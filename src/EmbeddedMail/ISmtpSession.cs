using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using EmbeddedMail.Handlers;

namespace EmbeddedMail
{
    public interface ISmtpSession : IDisposable
    {
        IEnumerable<string> Recipients { get; }
        string RemoteAddress { get; }
        void WriteResponse(string data);
        void SaveMessage(MailMessage message);
        void AddRecipient(string address);
    }

    public class SmtpSession : ISmtpSession
    {
        private readonly ISocket _socket;
        private StreamWriter _writer;
        private StreamReader _reader;
        private readonly IList<MailMessage> _messages = new List<MailMessage>();
        private readonly IList<string> _recipients = new List<string>(); 

        public SmtpSession(ISocket socket)
        {
            _socket = socket;
            OnMessage = (msg) => { };
        }

        public Action<MailMessage> OnMessage { get; set; }

        public void Start()
        {
            if (!_socket.Connected) return;

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

            _writer.WriteLine("220 localhost Server Ready");
            var isMessageBody = false;
            while(_socket.Connected)
            {
                var line = _reader.ReadLine();
                if (line == null) {
                    /*
                     * ReadLine returns `null` if the end of the input stream is reached
                     * https://docs.microsoft.com/en-us/dotnet/api/system.io.streamreader.readline
                     */
                    WriteResponse("421 localhost reached end of input stream while attempting to receive the next line from the client");
                    return;
                }
                var token = SmtpToken.FromLine(line, isMessageBody);
                
                SmtpLog.Debug(token.Data);

                var handler = ProtocolHandlers.HandlerFor(token);
                if(handler.Handle(token, this) == ContinueProcessing.Stop)
                {
                    return;
                }

                isMessageBody = token.IsData && token.IsMessageBody;
            }

            SmtpLog.Warn("The socket closed unexpectedly");
        }

        public void Dispose()
        {
            _writer.Close();
            _reader.Close();
        }

        public IEnumerable<string> Recipients
        {
            get { return _recipients.ToArray(); }
        }

        public string RemoteAddress
        {
            get { return _socket.RemoteIpAddress; }
        }

        public void WriteResponse(string data)
        {
            SmtpLog.Debug(data);
            _writer.WriteLine(data);
        }

        public void SaveMessage(MailMessage message)
        {
            _messages.Add(message);
            OnMessage(message);
        }

        public void AddRecipient(string address)
        {
            _recipients.Add(address);
        }
    }
}