// EDITED BY BLOCHER CONSULTING

namespace EmbeddedMail.Handlers
{
    public class QuitHandler : ISmtpProtocolHandler
    {
        public bool Matches(SmtpToken token)
        {
            return token.Command == "QUIT";
        }

        public ContinueProcessing Handle(SmtpToken token, ISmtpSession session, bool authorized)
        {
            session.WriteResponse("221 Bye");
            return ContinueProcessing.Stop;
        }
    }
}