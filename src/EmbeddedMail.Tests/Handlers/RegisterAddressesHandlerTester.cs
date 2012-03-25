using EmbeddedMail.Handlers;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace EmbeddedMail.Tests.Handlers
{
    [TestFixture]
    public class RegisterAddressesHandlerTester
    {
        private RegisterAddressesHandler theHandler;
        private ISmtpSession theSession;
        private SmtpToken theToken;

        [SetUp]
        public void SetUp()
        {
            theSession = MockRepository.GenerateStub<ISmtpSession>();
            theHandler = new RegisterAddressesHandler();
            theToken = new SmtpToken();
        }

        [Test]
        public void matches_mail_command()
        {
            theToken.Command = "MAIL FROM";
            theHandler.Matches(theToken).ShouldBeTrue();
        }

        [Test]
        public void matches_rcpt_command()
        {
            theToken.Command = "RCPT TO";
            theHandler.Matches(theToken).ShouldBeTrue();
        }

        [Test]
        public void does_not_match_other_commands()
        {
            theToken.Command = "BLAH";
            theHandler.Matches(theToken).ShouldBeFalse();
        }

        [Test]
        public void writes_ok_response()
        {
            theHandler.Handle(theToken, theSession);
            theSession.AssertWasCalled(x => x.WriteResponse("250 Ok"));
        }

        [Test]
        public void registers_the_recipient()
        {
            theToken.Command = "RCPT TO";
            theToken.Data = "RCPT TO:<user@domain.com>";
            theHandler.Handle(theToken, theSession);
            theSession.AssertWasCalled(x => x.AddRecipient("user@domain.com"));
        }

        [Test]
        public void should_continue()
        {
            theHandler.Handle(theToken, theSession).ShouldEqual(ContinueProcessing.Continue);
        }
    }
}