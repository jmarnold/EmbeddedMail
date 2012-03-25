using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace EmbeddedMail.Handlers
{
    public class MessageParsingHandler : ISmtpProtocolHandler
    {
        private readonly StringBuilder _messageGatherer = new StringBuilder();

        public bool Matches(SmtpToken token)
        {
            if(!token.IsMessageBody)
            {
                // Not the cleanest way to do this but this little guy maintains some state between commands so we clear it out here
                _messageGatherer.Length = 0;
                return false;
            }
            
            return true;
        }

        public ContinueProcessing Handle(SmtpToken token, ISmtpSession session)
        {
            _messageGatherer.AppendLine(token.Data);

            if(token.Data != null && token.Data.Trim() == ".")
            {
                session.WriteResponse(string.Format("250 Ok: queued as {0}", Guid.NewGuid()));
                session.SaveMessage(CreateMessage(_messageGatherer, session));
                token.IsMessageBody = false;
            }

            return ContinueProcessing.Continue;
        }

        public string CurrentMessage { get { return _messageGatherer.ToString(); } }

        public MailMessage CreateMessage(StringBuilder builder, ISmtpSession session)
        {
            var message = new MailMessage();
            var addresses = new List<string>();
            var lines = builder.ToString().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None);

            lines.Each(line =>
            {
                if(line.StartsWith("From"))
                {
                    message.From = new MailAddress(StringExtensions.ValueFromAttributeSyntax(line));
                }

                if(line.StartsWith("To"))
                {
                    StringExtensions.ValueFromAttributeSyntax(line)
                        .Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                        .Each(x =>
                        {
                            message.To.Add(x);
                            addresses.Add(x);
                        });
                }

                if(line.StartsWith("Cc"))
                {
                    StringExtensions.ValueFromAttributeSyntax(line)
                        .Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                        .Each(x =>
                        {
                            message.CC.Add(x);
                            addresses.Add(x);
                        });
                }

                if(line.StartsWith("Subject"))
                {
                    message.Subject = StringExtensions.ValueFromAttributeSyntax(line);
                }
            });

            var messageParts = builder.ToString().Split(new[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.None);
            if(messageParts.Length > 2)
            {
                message.Body = messageParts[1];
            }

            // Any recipients that are registered but haven't been used are blind copies
            session
                .Recipients
                .Where(x => !addresses.Contains(x))
                .Each(x => message.Bcc.Add(x));


            return message;
        }
    }
}