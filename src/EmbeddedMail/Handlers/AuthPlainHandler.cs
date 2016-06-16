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
        //This is where actual authentication should happen instead of auto-returning 235 success
        //For more SMTP protocol: http://www.samlogic.net/articles/smtp-commands-reference-auth.htm
        session.WriteResponse("235 OK");
        return ContinueProcessing.Continue;
      }
    }

    public virtual bool SmtpAuthorization(string username, string password) {
      return true;
    }
  }
}