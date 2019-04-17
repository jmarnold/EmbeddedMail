// EDITED BY BLOCHER CONSULTING

using System;
using System.IO;
using System.Linq;
using System.Text;
using MimeKit;

namespace EmbeddedMail.Handlers {
  public class MessageParsingHandler : ISmtpProtocolHandler {
    private readonly StringBuilder _messageGatherer = new StringBuilder();

    public bool Matches(SmtpToken token) {
      if (!token.IsMessageBody) {
        // Not the cleanest way to do this but this little guy maintains some state between commands so we clear it out here
        _messageGatherer.Length = 0;
        return false;
      }

      return true;
    }

    public ContinueProcessing Handle(SmtpToken token, ISmtpSession session, bool authorized) {
      if (!authorized) {
        session.WriteResponse("530 Authorization Required");
        return ContinueProcessing.ContinueAuth;
      }

      if (token.Data == ".") {
        try {
          session.SaveMessage(CreateMessage(_messageGatherer, session));
        } catch (Exception e) {
          SmtpLog.Logger.Warning(e, "Exception thrown while trying to save message");
          session.WriteResponse(string.Format("451 Requested action aborted: error in processing"));
        }
        token.IsMessageBody = false;
      } else {
        var newLine = token.Data[0] == '.'
          ? token.Data.Substring(1)
          : token.Data;
        _messageGatherer.AppendLine(newLine);
      }

      return ContinueProcessing.Continue;
    }

    public string CurrentMessage { get { return _messageGatherer.ToString(); } }

    public MimeMessage CreateMessage(StringBuilder builder, ISmtpSession session) {
      var message = MimeMessage.Load(new MemoryStream(Encoding.UTF8.GetBytes(builder.ToString())));
      //var message = new MessageParser().Parse(builder.ToString());

      // Any recipients that are registered but haven't been used are blind copies
      session
          .Recipients
          .Where(x => !message.To.Any(y => y.Name == x) && !message.Cc.Any(y => y.Name == x))
          .Each(x => message.Bcc.Add(InternetAddress.Parse(x)));

      return message;
    }
  }


}