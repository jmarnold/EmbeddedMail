using System.Linq;
using System.Net.Mail;
using FubuTestingSupport;
using NUnit.Framework;

namespace EmbeddedMail.Tests
{
    [TestFixture]
    public class IntegratedSingleMessageTester
    {
        private EmbeddedSmtpServer theServer;
        private MailMessage theMessage;
        private SmtpClient theClient;
        private const string ccAddress = "copy@domain.com";
        private const string bccAddress = "blind@domain.com";

        [TestFixtureSetUp]
        public void BeforeAll()
        {
            theServer = EmbeddedSmtpServer.Local(8181);

            theMessage = new MailMessage("x@domain.com", "y@domain.com", "Hello there",
                                            "O hai, here is a url for you: http://localhost/something/something/else/is/cool");
            theMessage.CC.Add(ccAddress);
            theMessage.Bcc.Add(bccAddress);
            theServer.Start();

            theClient = new SmtpClient("localhost", theServer.Port);
            theClient.Send(theMessage);

            theServer.WaitForMessages();
        }

        [TestFixtureTearDown]
        public void AfterAll()
        {
            theServer.Dispose();
            theClient.Dispose();
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
    }
}