using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using EmbeddedMail.Handlers;

namespace EmbeddedMail {
  public interface ISmtpSession : IDisposable {
    IEnumerable<string> Recipients { get; }
    string RemoteAddress { get; }
    void WriteResponse(string data);
    void SaveMessage(MailMessage message);
    void AddRecipient(string address);
  }

  public class SmtpSession : ISmtpSession {
    private readonly ISocket _socket;
    private StreamWriter _writer;
    private StreamReader _reader;
    private readonly IList<MailMessage> _messages = new List<MailMessage>();
    private readonly IList<string> _recipients = new List<string>();

    public SmtpSession(ISocket socket) {
      _socket = socket;
      OnMessage = new List<Action<MailMessage>>();
    }

    public List<Action<MailMessage>> OnMessage { get; private set; }

    public void Start() {
      if (!_socket.Connected) return;

      _reader = new StreamReader(_socket.Stream);
      _writer = new StreamWriter(_socket.Stream) { AutoFlush = true };

      _writer.WriteLine("220 localhost Server Ready");
      var isMessageBody = false;
      while (_socket.Connected) {
        SmtpToken token;
        try {
          token = SmtpToken.FromLine(_reader.ReadLine(), isMessageBody);
        } catch (IOException) {
          break;
        }

        SmtpLog.Debug(token.Data);
        var handler = ProtocolHandlers.HandlerFor(token);
        if (handler.Handle(token, this) == ContinueProcessing.Stop) {
          break;
        }
        isMessageBody = token.IsData && token.IsMessageBody;
      }

      try {
        _socket.Close();
        _socket.Dispose();
      } catch (Exception) { }
    }

    public void Dispose() {
      WriteResponse("421 localhost Closing transmission channel");

      _writer.Close();
      _reader.Close();
    }

    public IEnumerable<string> Recipients {
      get { return _recipients.ToArray(); }
    }

    public string RemoteAddress {
      get { return _socket.RemoteIpAddress; }
    }

    public void WriteResponse(string data) {
      SmtpLog.Debug(data);
      _writer.WriteLine(data);
    }

    public void SaveMessage(MailMessage message) {
      _messages.Add(message);
      OnMessage.ForEach(f => f(message));
    }

    public void AddRecipient(string address) {
      _recipients.Add(address);
    }
  }
}