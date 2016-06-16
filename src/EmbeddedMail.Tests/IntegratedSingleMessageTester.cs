using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using NUnit.Framework;
using Shouldly;

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
            theClient.UseDefaultCredentials = false;
            theClient.Credentials = new NetworkCredential("x@domain.com", "1234567890");

            try {
              theClient.Send(theMessage);
            } catch (Exception ex) {
              //Error, could not send the message
            }

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
            message.From.ShouldBe(theMessage.From);
            message.To.First().ShouldBe(theMessage.To.First());
            message.Subject.ShouldBe(theMessage.Subject);
            message.Body.ShouldBe(theMessage.Body);

            message.CC.Single().Address.ShouldBe(ccAddress);
            message.Bcc.Single().Address.ShouldBe(bccAddress);
        }
    }
}