using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EmbeddedMail.Handlers
{
    // Copyright (c) 2003, Eric Daugherty (http://www.ericdaugherty.com)
    // All rights reserved.
    // Modified by Carlos Mendible, Joshua Arnold
    public class SmtpMessagePart
    {
        private static readonly string DOUBLE_NEWLINE = Environment.NewLine + Environment.NewLine;

        private IDictionary<string, string> _headers;
        private string _headerData = String.Empty;
        private string _bodyData = String.Empty;

        public SmtpMessagePart(string data)
        {
            string[] parts = Regex.Split(data, DOUBLE_NEWLINE);

            _headerData = parts[0];
            _bodyData = parts[1];
        }


        public IDictionary<string, string> Headers
        {
            get
            {
                if (_headers == null)
                {
                    _headers = SmtpMessage.ParseHeaders(_headerData);
                }
                return _headers;
            }
        }

        public string HeaderData
        {
            get { return _headerData; }
        }

        public string BodyData
        {
            get { return _bodyData; }
        }
    }
}