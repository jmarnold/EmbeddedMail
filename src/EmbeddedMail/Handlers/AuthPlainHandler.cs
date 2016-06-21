using System;
using Serilog;
using System.Text;

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
      } else*/ if (!String.IsNullOrEmpty(token.Data) && token.Data == token.Command) {
        session.WriteResponse("334");
        return ContinueProcessing.ContinueAuth;
      } else if (!String.IsNullOrEmpty(token.Data) && token.Data.StartsWith(AUTH_PLAIN)) {
        try {
          var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token.Data.Split(' ')[2]));
          var email = decoded.Split('\0')[1];
          var password = decoded.Split('\0')[2];
          Log.Information("SMTP AUTH ATTEMPT!!! Email: '{0}' Password: '{1}'", email, password);
          //This is where actual authentication should happen instead of auto-returning 235 success
          //For more SMTP protocol: http://www.samlogic.net/articles/smtp-commands-reference-auth.htm

          //check email and password vs DB - 
          //235 + Continue if matches
          session.WriteResponse("235 OK");
          return ContinueProcessing.Continue;
          //534 wrong password + stop if not
        } catch (Exception ex) {
          Log.Debug("SMTP Authentication Error", ex);
          session.WriteResponse("535 Authentication Failed.");
          return ContinueProcessing.Stop;
        }


      } else {
        Log.Error("Unknown SMTP AUTH Protocol. Fix Required!");
        //535 auth failed?
        return ContinueProcessing.Stop;
      }
    }

    public virtual bool SmtpAuthorization(string username, string password) {
      return true;
    }
  }
}