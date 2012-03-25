namespace EmbeddedMail.Handlers
{
    public enum ContinueProcessing
    {
        Stop,
        Continue
    }

    public interface ISmtpProtocolHandler
    {
        bool Matches(SmtpToken token);
        ContinueProcessing Handle(SmtpToken token, ISmtpSession session);
    }
}