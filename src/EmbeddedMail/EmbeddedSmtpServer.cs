// EDITED BY BLOCHER CONSULTING

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using Serilog;

namespace EmbeddedMail {
  public interface ISmtpServer : IDisposable {
    IEnumerable<MimeMessage> ReceivedMessages();
    void Start();
    void Stop();
  }

  public class EmbeddedSmtpServer : ISmtpServer {
    private readonly ISmtpAuthorization _auth;
    private readonly IList<MimeMessage> _messages = new List<MimeMessage>();
    private readonly IList<ISmtpSession> _sessions = new List<ISmtpSession>();

    private int _connectedSessionCount = 0;
    private readonly object _connectedCountLock = new object();

    private bool _closed;

    public EmbeddedSmtpServer(int port = 25, ISmtpAuthorization auth = null, ILogger logger = null)
        : this(IPAddress.Any, port, auth, logger) {
    }

    public EmbeddedSmtpServer(IPAddress address, int port = 25, ISmtpAuthorization auth = null, ILogger logger = null) {
      this._auth = auth;
      Address = address;
      Port = port;
      Listener = new TcpListener(Address, port);
      SmtpLog.Logger = logger ?? new LoggerConfiguration().CreateLogger();
    }

    public TcpListener Listener { get; private set; }
    public IPAddress Address { get; private set; }
    public int Port { get; private set; }

    public IEnumerable<MimeMessage> ReceivedMessages() {
      return _messages;
    }

    public Action<SmtpSession> OnSessionStart { get; set; }

    public void Start() {
      Listener.Start();
      SmtpLog.Logger.Information("Server started at {IpEndPoint}", new IPEndPoint(Address, Port));
      ListenForClients();
    }

    public void WaitForMessages(int timeoutInMilliseconds = 5000) {
      var count = _messages.Count;
      Wait.Until(() => _messages.Count > count, timeoutInMilliseconds: timeoutInMilliseconds);
    }

    public void Stop() {
      _closed = true;
    }

    private void ListenForClients() {
      if (_closed) return;
      this.ListenForClients(OnClientConnect, OnClientException);
    }

    private void OnClientException(Exception exc) {
      if (exc is AggregateException) {
        var inner = exc.InnerException;
        if (inner != null) OnClientException(inner);
        return;
      }

      if (exc is ObjectDisposedException) return;

      SmtpLog.Logger.Warning(exc, "Listener socket is closed");
    }

    private Task ListenForClients(Action<ISocket> callback, Action<Exception> error) {
      try {
        Func<IAsyncResult, ISocket> end = r => new SocketWrapper(Listener.EndAcceptSocket(r));

        var task = Task.Factory.FromAsync(Listener.BeginAcceptSocket, end, null);

        task.ContinueWith(t => callback(t.Result), TaskContinuationOptions.NotOnFaulted)
            .ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);

        return task;

      } catch (Exception e) {
        error(e);
        return Task.Factory.StartNew(() => { });
      }
    }

    public void OnClientConnect(ISocket clientSocket) {
      if (_closed) return;

      ListenForClients();

      var session = new SmtpSession(clientSocket, this._auth);

      lock (_connectedCountLock) {
        _connectedSessionCount++;
      }
      SmtpLog.Logger.Debug("Client connected. Now {ConnectedClientCount} clients connected.", _connectedSessionCount);

      session.OnMessage.Add((m, ts) => _messages.Add(m));
      if (this.OnSessionStart != null) this.OnSessionStart(session);
      session.Start();


      lock (_connectedCountLock) {
        _connectedSessionCount--;
      }
      SmtpLog.Logger.Debug("Client disconnected. Now {ConnectedClientCount} clients connected.", _connectedSessionCount);

      _sessions.Add(session);
    }

    public void Dispose() {
      Stop();

      _sessions.Each(x => x.Dispose());

      /* TDW:
       * The comment below is from jmarnold.
       * Stopping the listener used to happen just before disposing each session.
       * I swapped these two because session disposal tries to write to the underlying socket,
       * which is disposed when the listener is stopped.
       */
      // I don't grok the disposal lifecycle for the sockets yet
      Listener.Stop();
    }

    /// <summary>
    /// Creates a new instance fot the <see cref="EmbeddedSmtpServer"/> class
    /// by finding the first open port starting at the specified port.
    /// </summary>
    /// <param name="startingPort">The port to start scanning from (default 25)</param>
    /// <returns></returns>
    public static EmbeddedSmtpServer Local(int startingPort = 8080) {
      var port = PortFinder.FindPort(startingPort);
      return new EmbeddedSmtpServer(port);
    }
  }

  public static class Wait {
    public static void Until(Func<bool> condition, int millisecondPolling = 500, int timeoutInMilliseconds = 5000) {
      if (condition()) return;

      var clock = new Stopwatch();
      clock.Start();

      while (clock.ElapsedMilliseconds < timeoutInMilliseconds) {
        Thread.Yield();
        Thread.Sleep(500);

        if (condition()) return;
      }
    }
  }
}