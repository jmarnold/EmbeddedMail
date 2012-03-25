using EmbeddedMail.Handlers;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace EmbeddedMail.Tests.Handlers
{
    [TestFixture]
    public class DataHandlerTester
    {
        private DataHandler theHandler;
        private ISmtpSession theSession;
        private SmtpToken theToken;

        [SetUp]
        public void SetUp()
        {
            theSession = MockRepository.GenerateStub<ISmtpSession>();
            theHandler = new DataHandler();
            theToken = new SmtpToken();
        }

        [Test]
        public void matches_data_command()
        {
            theToken.Command = "DATA";
            theHandler.Matches(theToken).ShouldBeTrue();
        }

        [Test]
        public void does_not_match_message_body()
        {
            theToken.Command = "DATA";
            theToken.IsMessageBody = true;

            theHandler.Matches(theToken).ShouldBeFalse();
        }

        [Test]
        public void does_not_match_other_commands()
        {
            theToken.Command = "BLAH";
            theToken.IsMessageBody = true;

            theHandler.Matches(theToken).ShouldBeFalse();
        }

        [Test]
        public void writes_end_character_response()
        {
            theHandler.Handle(theToken, theSession);
            theSession.AssertWasCalled(x => x.WriteResponse("354 End data with ."));
        }

        [Test]
        public void sets_the_message_body_flag()
        {
            theHandler.Handle(theToken, theSession);
            theToken.IsMessageBody.ShouldBeTrue();
        }

        [Test]
        public void should_continue()
        {
            theHandler.Handle(theToken, theSession).ShouldEqual(ContinueProcessing.Continue);
        }
    }
}