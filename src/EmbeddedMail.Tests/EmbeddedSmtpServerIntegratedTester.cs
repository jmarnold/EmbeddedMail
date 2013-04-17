using System.Linq;
using System.Net.Mail;
using FubuTestingSupport;
using NUnit.Framework;

namespace EmbeddedMail.Tests
{
    [TestFixture]
    public class EmbeddedSmtpServerIntegratedTester
    {
        private EmbeddedSmtpServer theServer;
        private MailMessage theMessage;
        private const string ccAddress = "copy@domain.com";
        private const string bccAddress = "blind@domain.com";

        [TestFixtureSetUp]
        public void BeforeAll()
        {
            theServer = EmbeddedSmtpServer.Local(8181);
            theMessage = new MailMessage("x@domain.com", "y@domain.com", "Hello there", "O hai, here is a url for you: http://localhost/something/something/else/is/cool");
            theMessage.CC.Add(ccAddress);
            theMessage.Bcc.Add(bccAddress);
            theServer.Start();

            using (var client = new SmtpClient("localhost", theServer.Port))
            {
                client.Send(theMessage);

                theMessage.CC.Clear();
                client.Send(theMessage);

                theMessage.Attachments.Add(new Attachment("Attachment1.txt"));
                theMessage.Attachments.Add(new Attachment("Attachment2.txt"));
                client.Send(theMessage);
            }

            theServer.Stop();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            theServer.Dispose();
        }

        [Test]
        public void receives_the_message()
        {
            var message = theServer.ReceivedMessages().First();
            message.From.ShouldEqual(theMessage.From);
            message.To.First().ShouldEqual(theMessage.To.First());
            message.Subject.ShouldEqual(theMessage.Subject);
            message.Body.ShouldEqual(theMessage.Body);

            message.CC.Single().Address.ShouldEqual(ccAddress);
            message.Bcc.Single().Address.ShouldEqual(bccAddress);
        }

        [Test]
        public void receives_multiple_messages()
        {
            theServer.ReceivedMessages().ShouldHaveCount(3);
        }

        [Test]
        public void receives_message_without_CC_header()
        {
            var message = theServer.ReceivedMessages().Skip(1).First();

            message.CC.Count.ShouldEqual(0);
        }

        [Test]
        public void receives_attachments()
        {
            theServer
                .ReceivedMessages()
                .Last()
                .Attachments
                .ShouldHaveCount(2);
        }
    }
}