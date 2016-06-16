using System.Linq;
using System.Net.Mail;
using System.Text;
using EmbeddedMail.Handlers;
using NUnit.Framework;
using Shouldly;

namespace EmbeddedMail.Tests.Handlers
{
    [TestFixture]
    public class when_parsing_a_simple_text_email
    {
        private StringBuilder theMessageBody;
        //private MessageParser theParser;
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

            //theParser = new MessageParser();
            //theMessage = theParser.Parse(theMessageBody.ToString());
        }

        [Test]
        public void parses_the_sender()
        {
            theMessage.From.Address.ShouldBe("x@domain.com");
        }

        [Test]
        public void parses_a_single_recipient()
        {
            theMessage.To.First().Address.ShouldBe("y@domain.com");
        }

        [Test]
        public void parses_multiple_recipients()
        {
            theMessage.To.Last().Address.ShouldBe("z@domain.com");
        }

        [Test]
        public void parses_the_subject()
        {
            theMessage.Subject.ShouldBe("This is a test");
        }

        [Test]
        public void parses_the_body()
        {
            theMessage.Body.ShouldBe("This is the body");
        }

        [Test]
        public void parses_copies()
        {
            theMessage.CC.Single().Address.ShouldBe("copy@domain.com");
        }
    }
}