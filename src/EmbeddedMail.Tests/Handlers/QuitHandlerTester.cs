using EmbeddedMail.Handlers;
using NUnit.Framework;
using Rhino.Mocks;
using Shouldly;

namespace EmbeddedMail.Tests.Handlers
{
    [TestFixture]
    public class QuitHandlerTester
    {
        private QuitHandler theHandler;
        private ISmtpSession theSession;
        private SmtpToken theToken;

        [SetUp]
        public void SetUp()
        {
            theSession = MockRepository.GenerateStub<ISmtpSession>();
            theHandler = new QuitHandler();
            theToken = new SmtpToken();
        }

        [Test]
        public void matches_quit_command()
        {
            theToken.Command = "QUIT";
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
            theSession.AssertWasCalled(x => x.WriteResponse("221 Bye"));
        }

        [Test]
        public void should_not_continue()
        {
            theHandler.Handle(theToken, theSession).ShouldBe(ContinueProcessing.Stop);
        }
    }
}