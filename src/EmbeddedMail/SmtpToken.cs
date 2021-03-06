using System.Collections.Generic;

namespace EmbeddedMail
{
    public class SmtpToken
    {
        public static readonly IEnumerable<string> KnownCommands;
        public static readonly string DataCommand = "DATA";

        static SmtpToken()
        {
            KnownCommands = new[] {"HELO", "EHLO", "MAIL FROM", "RCPT TO", "QUIT", DataCommand};
        }

        public bool IsData { get { return Command == DataCommand; } }
        public string Command { get; set; }
        public string Data { get; set; }
        public bool IsMessageBody { get; set; }

        public static SmtpToken FromLine(string line, bool isBody = false)
        {
            var command = DataCommand;
            foreach(var cmd in KnownCommands)
            {
                if(line.ToUpper().StartsWith(cmd))
                {
                    command = cmd;
                    break;
                }
            }

            return new SmtpToken
                       {
                           Command = command,
                           Data = line,
                           IsMessageBody = isBody
                       };
        }
    }
}