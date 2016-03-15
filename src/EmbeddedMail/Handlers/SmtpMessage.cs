using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Serilog;

namespace EmbeddedMail.Handlers
{
    // Based on work by Eric Daugherty and Carlos Mendible
    // Copyright (c) 2003, Eric Daugherty (http://www.ericdaugherty.com)
    // All rights reserved.
    // Modified by Carlos Mendible, Joshua Arnold
    public class SmtpMessage
    {
        private static readonly string DoubleNewline = Environment.NewLine + Environment.NewLine;

        private string _data;
        private IDictionary<string, string> _headers;

        public SmtpMessage(string data)
        {
            _data = data;
        }

        public IDictionary<string, string> Headers
        {
            get
            {
                if (_headers == null)
                {
                    _headers = ParseHeaders(_data);
                }

                return _headers;
            }
        }

        public IEnumerable<SmtpMessagePart> MessageParts
        {
            get
            {
                return parseMessageParts(_data);
            }
        }

        public static IDictionary<string, string> ParseHeaders(string partData)
        {
            IDictionary<string, string> headers = new Dictionary<string, string>();

            string[] parts = Regex.Split(partData, MessageParser.Newline_XP + MessageParser.Newline_XP);
            string headerString = parts[0] + DoubleNewline;

            var headerKeys = Regex.Matches(headerString, @"^(?<key>\S*):", RegexOptions.Multiline);
            string headerKey = null;
            foreach (Match match in headerKeys)
            {
                headerKey = match.Result("${key}");
                Match valueMatch = Regex.Match(headerString, headerKey + @":(?<value>.*?)" + MessageParser.Newline_XP + @"[\S\r]", RegexOptions.Singleline);
                if (valueMatch.Success)
                {
                    var headerValue = valueMatch.Result("${value}").Trim();
                    headerValue = Regex.Replace(headerValue, MessageParser.Newline_XP, "");
                    headerValue = Regex.Replace(headerValue, @"\s+", " ");
                    headers[headerKey] = headerValue;
                }
            }

            return headers;
        }

        private IEnumerable<SmtpMessagePart> parseMessageParts(string message)
        {
            Headers.Keys.Each(k => Log.Debug("DEBUG_PRINT Key '{0}' Value '{1}'", k, Headers[k]));

            string contentType = Headers["Content-Type"];

            // Check to see if it is a Multipart Messages
            if (contentType != null && Regex.Match(contentType, "multipart/mixed", RegexOptions.IgnoreCase).Success)
            {
                // Message parts are seperated by boundries.  Parse out what the boundry is so we can easily
                // parse the parts out of the message.
                Match boundryMatch = Regex.Match(contentType, "boundary=(?<boundry>\\S+)", RegexOptions.IgnoreCase);
                if (boundryMatch.Success)
                {
                    string boundry = boundryMatch.Result("${boundry}");

                    var messageParts = new List<SmtpMessagePart>();
                    MatchCollection matches = Regex.Matches(message, "--" + boundry + ".*" + MessageParser.Newline_XP);

                    int lastIndex = -1;
                    int currentIndex = -1;
                    int matchLength = -1;
                    string messagePartText = null;
                    foreach (Match match in matches)
                    {
                        currentIndex = match.Index;
                        matchLength = match.Length;

                        if (lastIndex != -1)
                        {
                            messagePartText = message.Substring(lastIndex, currentIndex - lastIndex);
                            messageParts.Add(new SmtpMessagePart(messagePartText));
                        }

                        lastIndex = currentIndex + matchLength;
                    }

                    return messageParts;
                }
            }
            else
            {
                return new[] { new SmtpMessagePart(message) };
            }

            return new SmtpMessagePart[0];
        }
    }
}