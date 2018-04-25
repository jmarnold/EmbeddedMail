// EDITED BY BLOCHER CONSULTING

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EmbeddedMail {
  // Shamelessly ripped out of Fleck. Thanks, Jason ;)
  public interface ISocket {
    string LocalIpAddress { get; }
    string RemoteIpAddress { get; }
    bool Connected { get; }
    bool DataAvailable { get; }
    Stream Stream { get; }

    Task<ISocket> Accept(Action<ISocket> callback, Action<Exception> error);
    Task Send(byte[] buffer, Action callback, Action<Exception> error);
    Task<int> Receive(byte[] buffer, Action<int> callback, Action<Exception> error, int offset = 0);

    void Dispose();
    void Close();

    void Bind(EndPoint ipLocal);
    void Listen(int backlog);
  }

  public class SocketWrapper : ISocket {
    private readonly Socket _socket;
    private Stream _stream;

    public string LocalIpAddress {
      get {
        var endpoint = _socket.LocalEndPoint as IPEndPoint;
        return endpoint != null ? endpoint.Address.ToString() : null;
      }
    }

    public string RemoteIpAddress {
      get {
        var endpoint = _socket.RemoteEndPoint as IPEndPoint;
        return endpoint != null ? endpoint.Address.ToString() : null;
      }
    }

    public SocketWrapper(Socket socket) {
      _socket = socket;
      if (_socket.Connected)
        _stream = new NetworkStream(_socket);
    }

    public void Listen(int backlog) {
      _socket.Listen(backlog);
    }

    public void Bind(EndPoint endPoint) {
      _socket.Bind(endPoint);
    }

    public bool Connected {
      get { return _socket.Connected; }
    }

    public bool DataAvailable {
      get { return ((NetworkStream) _stream).DataAvailable; }
    }

    public Stream Stream {
      get { return _stream; }
    }

    public Task<int> Receive(byte[] buffer, Action<int> callback, Action<Exception> error, int offset) {
      Func<AsyncCallback, object, IAsyncResult> begin =
          (cb, s) => _stream.BeginRead(buffer, offset, buffer.Length, cb, s);

      Task<int> task = Task.Factory.FromAsync<int>(begin, _stream.EndRead, null);
      task.ContinueWith(t => callback(t.Result), TaskContinuationOptions.NotOnFaulted)
          .ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
      task.ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
      return task;
    }

    public Task<ISocket> Accept(Action<ISocket> callback, Action<Exception> error) {
      Func<IAsyncResult, ISocket> end = r => new SocketWrapper(_socket.EndAccept(r));
      var task = Task.Factory.FromAsync(_socket.BeginAccept, end, null);
      task.ContinueWith(t => callback(t.Result), TaskContinuationOptions.NotOnFaulted)
          .ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
      task.ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
      return task;
    }

    public void Dispose() {
      if (_stream != null) _stream.Dispose();
      if (_socket != null) _socket.Dispose();
    }

    public void Close() {
      if (_stream != null) _stream.Close();
      if (_socket != null) _socket.Close();
    }

    public int EndSend(IAsyncResult asyncResult) {
      _stream.EndWrite(asyncResult);
      return 0;
    }

    public Task Send(byte[] buffer, Action callback, Action<Exception> error) {
      Func<AsyncCallback, object, IAsyncResult> begin =
          (cb, s) => _stream.BeginWrite(buffer, 0, buffer.Length, cb, s);

      Task task = Task.Factory.FromAsync(begin, _stream.EndWrite, null);
      task.ContinueWith(t => callback(), TaskContinuationOptions.NotOnFaulted)
          .ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
      task.ContinueWith(t => error(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
      return task;
    }
  }
}