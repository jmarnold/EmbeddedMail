using System.Linq;
using System.Net.Mail;
using System.Text;
using EmbeddedMail.Handlers;
using FubuTestingSupport;
using NUnit.Framework;

namespace EmbeddedMail.Tests.Handlers
{
    [TestFixture]
    public class when_parsing_a_simple_text_email
    {
        private StringBuilder theMessageBody;
        private MessageParser theParser;
        private MailMessage theMessage;

        [SetUp]
        public void SetUp()
        {
            theMessageBody = new StringBuilder();
            theMessageBody.AppendLine("MIME-Version: 1.0");
            theMessageBody.AppendLine("From: x@domain.com");
            theMessageBody.AppendLine("To: y@domain.com, z@domain.com");
            theMessageBody.AppendLine("Cc: copy@domain.com");
            theMessageBody.AppendLine("Subject: This is a test");
            theMessageBody.AppendLine("Content-Type: text/plain; charset=us-ascii");
            theMessageBody.AppendLine("Content-Transfer-Encoding: quoted-printable");
            theMessageBody.AppendLine();
            theMessageBody.AppendLine("This is the body");
            theMessageBody.AppendLine().AppendLine(".").AppendLine();

            theParser = new MessageParser();
            theMessage = theParser.Parse(theMessageBody.ToString());
        }

        [Test]
        public void parses_the_sender()
        {
            theMessage.From.Address.ShouldEqual("x@domain.com");
        }

        [Test]
        public void parses_a_single_recipient()
        {
            theMessage.To.First().Address.ShouldEqual("y@domain.com");
        }

        [Test]
        public void parses_multiple_recipients()
        {
            theMessage.To.Last().Address.ShouldEqual("z@domain.com");
        }

        [Test]
        public void parses_the_subject()
        {
            theMessage.Subject.ShouldEqual("This is a test");
        }

        [Test]
        public void parses_the_body()
        {
            theMessage.Body.ShouldEqual("This is the body");
        }

        [Test]
        public void parses_copies()
        {
            theMessage.CC.Single().Address.ShouldEqual("copy@domain.com");
        }
    }
}