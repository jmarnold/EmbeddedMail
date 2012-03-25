using EmbeddedMail.Handlers;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace EmbeddedMail.Tests.Handlers
{
    [TestFixture]
    public class HeloHandlerTester
    {
        private HeloHandler theHandler;
        private ISmtpSession theSession;
        private SmtpToken theToken;

        [SetUp]
        public void SetUp()
        {
            theSession = MockRepository.GenerateStub<ISmtpSession>();
            theHandler = new HeloHandler();
            theToken = new SmtpToken();

            theSession.Stub(x => x.RemoteAddress).Return("1234");
        }

        [Test]
        public void matches_helo_command()
        {
            theToken.Command = "HELO";
            theHandler.Matches(theToken).ShouldBeTrue();
        }

        [Test]
        public void matches_ehlo_command()
        {
            theToken.Command = "EHLO";
            theHandler.Matches(theToken).ShouldBeTrue();
        }

        [Test]
        public void does_not_match_other_commands()
        {
            theToken.Command = "BLAH";
            theHandler.Matches(theToken).ShouldBeFalse();
        }

        [Test]
        public void writes_hello_response()
        {
            theHandler.Handle(theToken, theSession);
            theSession.AssertWasCalled(x => x.WriteResponse("250 Hello 1234, I am glad to meet you"));
        }

        [Test]
        public void should_continue()
        {
            theHandler.Handle(theToken, theSession).ShouldEqual(ContinueProcessing.Continue);
        }
    }
}