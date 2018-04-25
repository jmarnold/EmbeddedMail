// EDITED BY BLOCHER CONSULTING

namespace EmbeddedMail.Handlers
{
    public class DataHandler : ISmtpProtocolHandler
    {
        public bool Matches(SmtpToken token)
        {
            return token.IsData && !token.IsMessageBody;
        }

        public ContinueProcessing Handle(SmtpToken token, ISmtpSession session, bool authorized)
        {
            if (!authorized) {
              session.WriteResponse("530 Authorization Required");
              return ContinueProcessing.ContinueAuth;
            }
            session.WriteResponse("354 End data with .");
            token.IsMessageBody = true;
            return ContinueProcessing.Continue;
        }
    }
}