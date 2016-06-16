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
        private const string from = "x@domain.com";
        private const string to = "y@domain.com";
        private const string subject = "Hello there";
        private const string body = "testBODY1289523";



        [TestFixtureSetUp]
        public void BeforeAll()
        {
            theServer = EmbeddedSmtpServer.Local(8181);

            theMessage = new MailMessage(from, to, subject, body);
            theMessage.CC.Add(ccAddress);
            theMessage.Bcc.Add(bccAddress);
            theServer.Start();

            theClient = new SmtpClient("127.0.0.1", theServer.Port);
            theClient.UseDefaultCredentials = false;
            theClient.Credentials = new NetworkCredential("x", "1234567890");

            
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
            message.From.ShouldBe(theMessage.From);
            message.To.First().ShouldBe(theMessage.To.First());
            message.Subject.ShouldBe(theMessage.Subject);
            message.Body.Substring(0,theMessage.Body.Length).ShouldBe(theMessage.Body);
            //Above line is a fix for erroneous cross-platform line ending conflicts
            message.CC.Single().Address.ShouldBe(ccAddress);
            message.Bcc.Single().Address.ShouldBe(bccAddress);
        }
    }
}