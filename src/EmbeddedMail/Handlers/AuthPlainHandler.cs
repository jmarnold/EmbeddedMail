using System;

namespace EmbeddedMail.Handlers {
  public class AuthPlainHandler : ISmtpProtocolHandler {
    public bool Matches(SmtpToken token) {
      return token.Command == "AUTH PLAIN";
    }

    public ContinueProcessing Handle(SmtpToken token, ISmtpSession session) {
      if (!String.IsNullOrEmpty(token.Data) && token.Data == token.Command) {
        session.WriteResponse("334");
        return ContinueProcessing.ContinueAuth;
      } else {
        session.WriteResponse("235 OK");
        return ContinueProcessing.Continue;
      }
    }
  }
}