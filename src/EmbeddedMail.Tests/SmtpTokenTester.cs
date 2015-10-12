using System;
using NUnit.Framework;
using Shouldly;

namespace EmbeddedMail.Tests
{
    [TestFixture]
    public class SmtpTokenTester
    {
        [Test]
        public void should_parse_known_commands()
        {
            verifyToken("HELO localhost", token => token.Command.ShouldBe("HELO"));
            verifyToken("EHLO localhost", token => token.Command.ShouldBe("EHLO"));
            verifyToken("MAIL FROM: jmarnold@home.net", token => token.Command.ShouldBe("MAIL FROM"));
            verifyToken("RCPT TO: you@there.com", token => token.Command.ShouldBe("RCPT TO"));
            verifyToken("DATA", token => token.Command.ShouldBe("DATA"));
            verifyToken("Subject: Test message", token => token.Command.ShouldBe("DATA"));
        }

        private void verifyToken(string input, Action<SmtpToken> action)
        {
            action(SmtpToken.FromLine(input));
        }
    }
}