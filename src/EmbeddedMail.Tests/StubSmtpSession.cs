using System.Collections.Generic;
using System.Net.Mail;

namespace EmbeddedMail.Tests
{
    public class StubSmtpSession : ISmtpSession
    {
        public void Dispose()
        {
        }

        public IEnumerable<string> Recipients
        {
            get { return new string[0]; }
        }

        public string RemoteAddress
        {
            get { return "localhost"; }
        }

        public void WriteResponse(string data)
        {
        }

        public void SaveMessage(MailMessage message)
        {
        }

        public void AddRecipient(string address)
        {
        }
    }
}