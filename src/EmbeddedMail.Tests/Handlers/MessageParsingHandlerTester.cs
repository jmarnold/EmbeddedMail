using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using EmbeddedMail.Handlers;
using NUnit.Framework;
using Rhino.Mocks;
using Shouldly;

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
                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
				.ToList()
                .ForEach(line =>
                {
                    if (line.Trim() == ".") return;

                    var token = SmtpToken.FromLine(line);
                    theHandler.Handle(token, theSession);
                });

            theHandler.CurrentMessage.ShouldNotBeEmpty();

            theHandler.Matches(new SmtpToken {IsMessageBody = false});
            theHandler.CurrentMessage.ShouldBeEmpty();
        }

        // protocol got weird here
        [Test]
        public void uses_unmentioned_addresses_that_are_registered_as_blind_copies()
        {
            theRecipients.Add("blind@domain.com");
            theMessage = theHandler.CreateMessage(theMessageBody, theSession);

            theMessage.Bcc.Single().Address.ShouldBe("blind@domain.com");
        }
    }
}