// EDITED BY BLOCHER CONSULTING

using System;
using Serilog;
using System.Text;
using System.Linq;

namespace EmbeddedMail.Handlers {
  public class AuthPlainHandler : ISmtpProtocolHandler {
    public const string AUTH_PLAIN = "AUTH PLAIN";
    protected readonly ISmtpAuthorization _auth;
    public AuthPlainHandler(ISmtpAuthorization auth) {
      this._auth = auth;
    }
    public bool Matches(SmtpToken token) {
      return token.Command == AUTH_PLAIN;
    }

    public ContinueProcessing Handle(SmtpToken token, ISmtpSession session, bool authorized) {
      /*if (authorized) { //Do NOT skip this /w authorized or it breaks SMTP clients
        session.WriteResponse("235 OK");
        return ContinueProcessing.Continue;
      } else*/
      if (!String.IsNullOrEmpty(token.Data) && token.Data == token.Command) {
        session.WriteResponse("334");
        return ContinueProcessing.ContinueAuth;
      } else if (!String.IsNullOrEmpty(token.Data) && token.Data.StartsWith(AUTH_PLAIN)) {
        var encoded = token.Data.Split(' ')[2];
        if (encoded.Length == 0)
          encoded = token.Data.Split(' ')[3];
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        var email = decoded.Split('\0')[1];
        var password = decoded.Split('\0')[2];
        //This is where actual authentication should happen instead of auto-returning 235 success
        //For more SMTP protocol: http://www.samlogic.net/articles/smtp-commands-reference-auth.htm

        var authorizationEmailAddresses = _auth.GetAuthorizedEmailAddresses(email, password);
        session.AuthorizationEmailAddresses = authorizationEmailAddresses;
        if (authorizationEmailAddresses.Any()) {
          session.WriteResponse("235 OK");
          return ContinueProcessing.Continue;
        } else {
          session.WriteResponse("535 Authentication failed. Restarting authentication process."); //from hMailServer
          return ContinueProcessing.ContinueAuth;
        }
      } else {
        Log.Error("Unknown SMTP AUTH Protocol. Fix Required!");
        session.WriteResponse("504 Authentication mechanism not supported"); //from hMailServer
        return ContinueProcessing.Stop;
      }
    }

    public virtual bool SmtpAuthorization(string username, string password) {
      return true;
    }
  }
}