// EDITED BY BLOCHER CONSULTING

namespace EmbeddedMail.Handlers {
  public class EhloHandler : ISmtpProtocolHandler {
    public bool Matches(SmtpToken token) {
      return token.Command == "EHLO";
    }

    public ContinueProcessing Handle(SmtpToken token, ISmtpSession session, bool authorized) {
      //Authorization doesn't matter at this stage, ignore authorized.
      session.WriteResponse(string.Format("250-Hello {0}, I am glad to meet you\r\n250 AUTH PLAIN", session.RemoteAddress));
      return ContinueProcessing.Continue;
    }
  }
}