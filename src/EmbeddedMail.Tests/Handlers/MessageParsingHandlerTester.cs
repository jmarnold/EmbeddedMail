using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using EmbeddedMail.Handlers;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace EmbeddedMail.Tests.Handlers
{
    [TestFixture]
    public class MessageParsingHandlerTester
    {
        private MessageParsingHandler theHandler;
        private StringBuilder theMessageBody;
        private MailMessage theMessage;
        private ISmtpSession theSession;
        private IList<string> theRecipients;

        [SetUp]
        public void SetUp()
        {
            theHandler = new MessageParsingHandler();

            theMessageBody = new StringBuilder();
            theMessageBody.AppendLine("MIME-TYPE: blah");
            theMessageBody.AppendLine("From: x@domain.com");
            theMessageBody.AppendLine("To: y@domain.com, z@domain.com");
            theMessageBody.AppendLine("Cc: copy@domain.com");
            theMessageBody.AppendLine("Subject: This is a test");
            theMessageBody.AppendLine();
            theMessageBody.AppendLine("This is the body");
            theMessageBody.AppendLine().AppendLine(".").AppendLine();

            theRecipients = new List<string>();
            theSession = MockRepository.GenerateStub<ISmtpSession>();

            theSession.Stub(x => x.Recipients).Return(theRecipients);

            theMessage = theHandler.CreateMessage(theMessageBody, theSession);
        }

        [Test]
        public void does_not_match_when_is_not_message_body()
        {
            var token = new SmtpToken();
            theHandler.Matches(token).ShouldBeFalse();
        }

        [Test]
        public void matches_when_is_message_body()
        {
            var token = new SmtpToken { IsMessageBody = true};
            theHandler.Matches(token).ShouldBeTrue();
        }

        [Test]
        public void maintains_message_state()
        {
            theMessageBody
                .ToString()
                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None)
                .Each(line =>
                {
                    var token = SmtpToken.FromLine(line);
                    theHandler.Handle(token, theSession);
                });

            theHandler.CurrentMessage.ShouldNotBeEmpty();

            theHandler.Matches(new SmtpToken {IsMessageBody = false});
            theHandler.CurrentMessage.ShouldBeEmpty();
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

        // protocol got weird here
        [Test]
        public void uses_unmentioned_addresses_that_are_registered_as_blind_copies()
        {
            theRecipients.Add("blind@domain.com");
            theMessage = theHandler.CreateMessage(theMessageBody, theSession);

            theMessage.Bcc.Single().Address.ShouldEqual("blind@domain.com");
        }
    }
}