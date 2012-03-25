using System;
using FubuTestingSupport;
using NUnit.Framework;

namespace EmbeddedMail.Tests
{
    [TestFixture]
    public class SmtpTokenTester
    {
        [Test]
        public void should_parse_known_commands()
        {
            verifyToken("HELO localhost", token => token.Command.ShouldEqual("HELO"));
            verifyToken("EHLO localhost", token => token.Command.ShouldEqual("EHLO"));
            verifyToken("MAIL FROM: jmarnold@home.net", token => token.Command.ShouldEqual("MAIL FROM"));
            verifyToken("RCPT TO: you@there.com", token => token.Command.ShouldEqual("RCPT TO"));
            verifyToken("DATA", token => token.Command.ShouldEqual("DATA"));
            verifyToken("Subject: Test message", token => token.Command.ShouldEqual("DATA"));
        }

        private void verifyToken(string input, Action<SmtpToken> action)
        {
            action(SmtpToken.FromLine(input));
        }
    }
}