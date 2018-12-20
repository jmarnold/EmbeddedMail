// EDITED BY BLOCHER CONSULTING

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using EmbeddedMail.Handlers;
using MimeKit;
using Serilog;

namespace EmbeddedMail {
  public interface ISmtpSession : IDisposable {
    IEnumerable<string> AuthorizationEmailAddresses { get; set; }
    IEnumerable<string> Recipients { get; }
    string RemoteAddress { get; }
    void WriteResponse(string data);
    void SaveMessage(MimeMessage message);
    void AddRecipient(string address);
  }

  public class SmtpSession : ISmtpSession {

    private readonly ILogger _log = SmtpLog.Logger.ForContext<SmtpSession>();

    protected readonly ISmtpAuthorization _auth;
    private readonly ISocket _socket;
    private StreamWriter _writer;
    private StreamReader _reader;
    private readonly IList<MimeMessage> _messages = new List<MimeMessage>();
    private readonly IList<string> _recipients = new List<string>();

    public SmtpSession(ISocket socket, ISmtpAuthorization auth) {
      this._auth = auth;
      _socket = socket;
      OnMessage = new List<Action<MimeMessage, IEnumerable<string>>>();
    }

    public List<Action<MimeMessage, IEnumerable<string>>> OnMessage { get; private set; }
    public IEnumerable<string> AuthorizationEmailAddresses { get; set; } = new List<string>();

    public void Start() {
      if (!_socket.Connected) return;

      _socket.Stream.ReadTimeout = 10000;
      _socket.Stream.WriteTimeout = 10000;

      _reader = new StreamReader(
        stream: _socket.Stream,
        // the next three values are the defaults when calling the overload that just takes a Stream
        encoding: Encoding.UTF8,
        detectEncodingFromByteOrderMarks: true,
        bufferSize: 1024,
        // the prevoius three values are the defaults when calling the overload that just takes a Stream
        leaveOpen: true // don't dispose objects given to you
      );

      Encoding GetDefaultStreamWriterEncoding() {
        using (var writer = new StreamWriter(Stream.Null)) {
          return writer.Encoding;
        }
      }

      _writer = new StreamWriter(
        stream: _socket.Stream,
        // the next two values are the defaults when calling the overload that just takes a Stream
        encoding: GetDefaultStreamWriterEncoding(),
        bufferSize: 1024,
        // the previous two values are the defaults when calling the overload that just takes a Stream
        leaveOpen: true // don't dispose objects given to you
      ) { AutoFlush = true, NewLine = "\r\n" };

      var greeting = String.Format("220 {0} ESMTP", _socket.LocalIpAddress);
      _writer.WriteLine(greeting);
      _log.Debug("S: {Message}", greeting);

      var handlers = new ProtocolHandlers(this._auth);
      var authorized = false;
      var isMessageBody = false;
      var dataReceived = false;
      while (_socket.Connected) {
        SmtpToken token;
        try {
          var line = _reader.ReadLine();
          _log.Debug("C: {Message}", line);
          token = SmtpToken.FromLine(line, isMessageBody);
        } catch (IOException) {
          break;
        }
        
        var handler = handlers.HandlerFor(token);
        var cp = handler.Handle(token, this, authorized);
        if (cp == ContinueProcessing.Stop) {
          break;
        } else if (cp == ContinueProcessing.Continue && handler is AuthPlainHandler) {
          authorized = true;
        } else if (cp == ContinueProcessing.ContinueAuth) {
          try {
            var line = _reader.ReadLine();
            _log.Debug("C: {Message}", line);
            if (new AuthPlainHandler(this._auth).Handle(new SmtpToken() { Data = line }, this, authorized) == ContinueProcessing.Continue) {
              authorized = true;
            }
          } catch(Exception e) { //If anything goes wrong here its likely the client clicking cancel. We dont care
            SmtpLog.Logger.Warning(e, "An exception occurred in AuthPlainHandler. Client likely cancelled connection.");
          }
        }

        // detect if done with DATA command; set timeout = 5 seconds afterwards.
        dataReceived = isMessageBody;
        isMessageBody = token.IsData && token.IsMessageBody;
        if (dataReceived && !isMessageBody) {
          _socket.Stream.ReadTimeout = 5000;
        }
      }
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
      _log.Debug("S: {Message}", data);
      _writer.WriteLine(data);
    }

    public void SaveMessage(MimeMessage message) {
      _messages.Add(message);
      OnMessage.ForEach(f => f(message, AuthorizationEmailAddresses));
    }

    public void AddRecipient(string address) {
      _recipients.Add(address);
    }
  }
}