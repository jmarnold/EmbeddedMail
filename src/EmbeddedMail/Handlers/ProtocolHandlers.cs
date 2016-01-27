using System.Collections.Generic;
using System.Linq;

namespace EmbeddedMail.Handlers {
  public class ProtocolHandlers {
    private static readonly IList<ISmtpProtocolHandler> Handlers;

    static ProtocolHandlers() {
      Handlers = new List<ISmtpProtocolHandler>();
      RegisterDefaults();
    }

    public static void RegisterDefaults() {
      Handlers.Clear();
      Handlers.Add(new HeloHandler());
      Handlers.Add(new EhloHandler());
      Handlers.Add(new AuthPlainHandler());
      Handlers.Add(new QuitHandler());
      Handlers.Add(new RegisterAddressesHandler());
      Handlers.Add(new DataHandler());
      Handlers.Add(new MessageParsingHandler());
    }

    public static void RegisterHandler(ISmtpProtocolHandler handler) {
      Handlers.Add(handler);
    }

    public static ISmtpProtocolHandler HandlerFor(SmtpToken token) {
      return Handlers.LastOrDefault(h => h.Matches(token));
    }
  }
}