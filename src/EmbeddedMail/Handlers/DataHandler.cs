namespace EmbeddedMail.Handlers
{
    public class DataHandler : ISmtpProtocolHandler
    {
        public bool Matches(SmtpToken token)
        {
            return token.IsData && !token.IsMessageBody;
        }

        public ContinueProcessing Handle(SmtpToken token, ISmtpSession session)
        {
            session.WriteResponse("354 End data with .");
            token.IsMessageBody = true;
            return ContinueProcessing.Continue;
        }
    }
}