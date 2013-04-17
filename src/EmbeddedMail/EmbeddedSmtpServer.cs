using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EmbeddedMail
{
    public interface ISmtpServer : IDisposable
    {
        IEnumerable<MailMessage> ReceivedMessages();
        void Start();
        void Stop();
    }

    public class EmbeddedSmtpServer : ISmtpServer
    {
        private readonly IList<MailMessage> _messages = new List<MailMessage>();
        private readonly IList<ISmtpSession> _sessions = new List<ISmtpSession>(); 
        private bool _closed;

        public EmbeddedSmtpServer(int port = 25)
            : this(IPAddress.Any, port)
        {
        }

        public EmbeddedSmtpServer(IPAddress address, int port = 25)
        {
            Address = address;
            Port = port;

            Listener = new TcpListener(Address, port);
        }

        public TcpListener Listener { get; private set; }
        public IPAddress Address { get; private set; }
        public int Port { get; private set; }

        public IEnumerable<MailMessage> ReceivedMessages()
        {
            return _messages;
        }

        public void Start()
        {
            Listener.Start();
            SmtpLog.Info(string.Format("Server started at {0}", new IPEndPoint(Address, Port)));
            ListenForClients();
        }

        public void WaitForMessages(int timeoutInMilliseconds = 5000)
        {
            var count = _messages.Count;
            Wait.Until(() => _messages.Count > count, timeoutInMilliseconds: timeoutInMilliseconds);
        }

        public void Stop()
        {
            _closed = true;
        }

        public void ListenForClients()
        {
            if (_closed) return;
            ListenForClients(OnClientConnect, e =>
            {
                if (e is ObjectDisposedException) return;
                SmtpLog.Error("Listener socket is closed", e);
            });
        }

        public Task<ISocket> ListenForClients(Action<ISocket> callback, Action<Exception> error)
        {
            Func<IAsyncResult, ISocket> end = r => new SocketWrapper(Listener.EndAcceptSocket(r));

            var task = Task.Factory.FromAsync(Listener.BeginAcceptSocket, end, null);
            
            task.ContinueWith(t => callback(t.Result), TaskContinuationOptions.NotOnFaulted)
                .ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
            
            return task;
        }

        public void OnClientConnect(ISocket clientSocket)
        {
            SmtpLog.Info("Client connected");
            ListenForClients();

            var session = new SmtpSession(clientSocket)
            {
                OnMessage = (msg) => _messages.Add(msg)
            };
            session.Start();

            _sessions.Add(session);
        }

        public void Dispose()
        {
            Stop();
            // I don't grok the disposal lifecycle for the sockets yet
            Listener.Stop();

            _sessions.Each(x => x.Dispose());
        }

        /// <summary>
        /// Creates a new instance fot the <see cref="EmbeddedSmtpServer"/> class
        /// by finding the first open port starting at the specified port.
        /// </summary>
        /// <param name="startingPort">The port to start scanning from (default 25)</param>
        /// <returns></returns>
        public static EmbeddedSmtpServer Local(int startingPort = 8080)
        {
            var port = PortFinder.FindPort(startingPort);
            return new EmbeddedSmtpServer(port);
        }
    }

    public static class Wait
    {
        public static void Until(Func<bool> condition, int millisecondPolling = 500, int timeoutInMilliseconds = 5000)
        {
            if (condition()) return;

            var clock = new Stopwatch();
            clock.Start();

            while (clock.ElapsedMilliseconds < timeoutInMilliseconds)
            {
                Thread.Yield();
                Thread.Sleep(500);

                if (condition()) return;
            }
        }
    }
}