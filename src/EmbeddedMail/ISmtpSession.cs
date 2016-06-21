using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    protected readonly ISmtpAuthorization _auth;
    private readonly ISocket _socket;
    private StreamWriter _writer;
    private StreamReader _reader;
    private readonly IList<MailMessage> _messages = new List<MailMessage>();
    private readonly IList<string> _recipients = new List<string>();

    public SmtpSession(ISocket socket, ISmtpAuthorization auth) {
      this._auth = auth;
      _socket = socket;
      OnMessage = new List<Action<MailMessage>>();
    }

    public List<Action<MailMessage>> OnMessage { get; private set; }

    public void Start() {
      if (!_socket.Connected) return;

      _socket.Stream.ReadTimeout = 10000;
      _socket.Stream.WriteTimeout = 10000;
      _reader = new StreamReader(_socket.Stream);
      _writer = new StreamWriter(_socket.Stream) { AutoFlush = true, NewLine = "\r\n" };

      var greeting = String.Format("220 {0} ESMTP", _socket.LocalIpAddress);
      _writer.WriteLine(greeting);
      SmtpLog.Debug(greeting);

      var isMessageBody = false;
      var dataReceived = false;
      while (_socket.Connected) {
        SmtpToken token;
        try {
          token = SmtpToken.FromLine(_reader.ReadLine(), isMessageBody);
        } catch (IOException) {
          break;
        }

        if (!String.IsNullOrWhiteSpace(token.Data)) SmtpLog.Info(token.Data ?? "");
        var authorized = false;
        var handler = ProtocolHandlers.HandlerFor(token);
        var cp = handler.Handle(token, this, authorized);
        if (cp == ContinueProcessing.Stop) {
          break;
        } else if (cp == ContinueProcessing.ContinueAuth) {
          if(new AuthPlainHandler(this._auth).Handle(new SmtpToken() { Data = _reader.ReadLine() }, this, authorized) == ContinueProcessing.Continue) {
            authorized = true;
          }
        }

        // detect if done with DATA command; set timeout = 5 seconds afterwards.
        dataReceived = isMessageBody;
        isMessageBody = token.IsData && token.IsMessageBody;
        if (dataReceived && !isMessageBody) {
          _socket.Stream.ReadTimeout = 5000;
        }
      }

      try {
        _socket.Close();
        _socket.Dispose();
      } catch (Exception) { }
    }

    public void Dispose() {
      WriteResponse(String.Format("421 {0}", Dns.GetHostName()));
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