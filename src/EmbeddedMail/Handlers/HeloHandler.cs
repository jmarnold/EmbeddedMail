namespace EmbeddedMail.Handlers {
  public class HeloHandler : ISmtpProtocolHandler {
    public bool Matches(SmtpToken token) {
      return token.Command == "HELO";
    }

    public ContinueProcessing Handle(SmtpToken token, ISmtpSession session, bool authorized) {
      //Authorization doesn't matter at this stage, ignore authorized.
      session.WriteResponse(string.Format("250 Hello {0}, I am glad to meet you", session.RemoteAddress));
      return ContinueProcessing.Continue;
    }
  }
}