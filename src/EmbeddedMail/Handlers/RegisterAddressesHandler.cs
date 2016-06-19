using System.Linq;

namespace EmbeddedMail.Handlers {
  public class RegisterAddressesHandler : ISmtpProtocolHandler {
    private const string Recipient = "RCPT TO";

    public bool Matches(SmtpToken token) {
      return (new[] { "MAIL FROM", Recipient }).Contains(token.Command);
    }

    public ContinueProcessing Handle(SmtpToken token, ISmtpSession session, bool authorized) {
      if (!authorized) {
        session.WriteResponse("530 Authorization Required");
        return ContinueProcessing.ContinueAuth;
      }
      if (token.Command == Recipient) {
        var rawAddress = token.Data.ValueFromAttributeSyntax(); // <user@domain.com>
        session.AddRecipient(rawAddress.Replace("<", "").Replace(">", ""));
      }

      session.WriteResponse("250 OK");
      return ContinueProcessing.Continue;
    }
  }
}