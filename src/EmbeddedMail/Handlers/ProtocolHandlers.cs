// EDITED BY BLOCHER CONSULTING

using System.Collections.Generic;
using System.Linq;

namespace EmbeddedMail.Handlers {
  public class ProtocolHandlers {
    private readonly IList<ISmtpProtocolHandler> Handlers;

    public ProtocolHandlers(ISmtpAuthorization auth) {
      Handlers = new List<ISmtpProtocolHandler>();
      Handlers.Clear();
      Handlers.Add(new HeloHandler());
      Handlers.Add(new EhloHandler());
      Handlers.Add(new AuthPlainHandler(auth));
      Handlers.Add(new QuitHandler());
      Handlers.Add(new RegisterAddressesHandler());
      Handlers.Add(new DataHandler());
      Handlers.Add(new MessageParsingHandler());
    }

    public void RegisterHandler(ISmtpProtocolHandler handler) {
      Handlers.Add(handler);
    }

    public ISmtpProtocolHandler HandlerFor(SmtpToken token) {
      return Handlers.LastOrDefault(h => h.Matches(token));
    }
  }
}