namespace EmbeddedMail.Handlers {
  public enum ContinueProcessing {
    Stop,
    Continue,
    ContinueAuth
  }

  public interface ISmtpProtocolHandler {
    bool Matches(SmtpToken token);
    ContinueProcessing Handle(SmtpToken token, ISmtpSession session);
  }
}