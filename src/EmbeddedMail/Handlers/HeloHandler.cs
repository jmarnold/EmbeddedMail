using System.Linq;

namespace EmbeddedMail.Handlers
{
    public class HeloHandler : ISmtpProtocolHandler
    {
        public bool Matches(SmtpToken token)
        {
            return (new[] {"HELO", "EHLO"}).Contains(token.Command);
        }

        public ContinueProcessing Handle(SmtpToken token, ISmtpSession session)
        {
            session.WriteResponse(string.Format("250 Hello {0}, I am glad to meet you", session.RemoteAddress));
            return ContinueProcessing.Continue;
        }
    }
}