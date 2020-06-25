using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetworkProgramming.Lab2
{
   public abstract class AbstractClient
   {
      protected abstract string OnSendErrorMessage { get; }
      protected abstract string OnConnectErrorMessage { get; }
      protected abstract string OnConnectSuccessMessage { get; }
      protected abstract string OnDisconnectSuccessMessage { get; }
      protected abstract string OnDisconnectErrorMessage { get; }
      protected abstract string OnReceiveErrorMessage { get; }
      private const int MaxLen = 1024;

      private SocketError error = SocketError.AccessDenied;
      private readonly ManualResetEvent _done;
      private readonly Socket _socket;
      private readonly string _address;
      private readonly int _port;
      public event EventHandler<object[]> OnLogEvent;

      protected AbstractClient(string address, int port, ManualResetEvent manualResetEvent)
      {
         _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
         _address = address;
         _port = port;
         _done = manualResetEvent;
         Connect();

      }

      protected AbstractClient(Socket connectedSocket, ManualResetEvent manualResetEvent = null)
      {
         _socket = connectedSocket;
         _done = manualResetEvent;
         Receive(_socket);
      }

      private void Connect()
      {
         try
         {
            _socket.BeginConnect(_address, _port, OnConnectedCallback, _socket);
         }
         catch (Exception e)
         {
            if (_socket.IsDisposed()) return;
            OnLogEvent?.Invoke(this, new object[] { (int)Logger.MessageType.Error, e, OnConnectErrorMessage });
            Disconnect();
         }
      }


      public bool IsConnected() =>
         !(_socket.IsDisposed() || (_socket.Poll(1000, SelectMode.SelectRead) && (_socket.Available == 0)) || !_socket.Connected);

      private void OnConnectedCallback(IAsyncResult ar)
      {
         try
         {
            var socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            socket.SendTimeout = 2000;
            Receive(socket);
            OnLogEvent?.Invoke(this, new object[] { (int)Logger.MessageType.Success, OnConnectSuccessMessage });
            _done?.Set();
         }
         catch (Exception e)
         {
            if (_socket.IsDisposed()) return;
            OnLogEvent?.Invoke(this, new object[] { (int)Logger.MessageType.Error, e, OnConnectErrorMessage });
            Disconnect();
         }
      }

      private void Receive(Socket socket)
      {
         var state = new ControlState
         {
            CurrentSocket = socket,
            Buffer = new byte[MaxLen],
            BufferSize = MaxLen
         };

         try
         {
            socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0,
               ReceiveCallback, state);
         }
         catch (Exception exception)
         {
            if (_socket.IsDisposed()) return;
            OnLogEvent?.Invoke(this, new object[] { (int)Logger.MessageType.Error, exception, OnReceiveErrorMessage });
            Disconnect();
         }
      }

      private void ReceiveCallback(IAsyncResult ar)
      {
         var state = (ControlState)ar.AsyncState;
         var client = state.CurrentSocket;
         try
         {
            var bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
               state.StreamBuffer.Write(state.Buffer, 0, bytesRead);
               if (state.Buffer.Any(byte_ => byte_ == '\0'))
               {
                  ProcessMessage(state.StreamBuffer);
                  state.StreamBuffer = new MemoryStream();
               }

            }
            else if (state.StreamBuffer.CanWrite && state.StreamBuffer.Length > 0)
            {
               ProcessMessage(state.StreamBuffer);
               state.StreamBuffer = new MemoryStream();
            }
         }
         catch (Exception s)
         {
            if (_socket.IsDisposed()) return;
            OnLogEvent?.Invoke(this, new object[] { (int)Logger.MessageType.Error, s, OnReceiveErrorMessage });
            Disconnect();
         }

         state.Buffer = new byte[state.BufferSize];

         try
         {
            client.BeginReceive(state.Buffer, 0, (int)state.BufferSize, 0, ReceiveCallback, state);
         }
         catch (Exception exception)
         {
            if (_socket.Connected)
            {
               OnLogEvent?.Invoke(this, new object[] { (int)Logger.MessageType.Error, exception, OnReceiveErrorMessage });
            }

            Disconnect();
         }

      }

      private void ProcessMessage(MemoryStream memory)
      {
         using var stream = memory;
         stream.Seek(0, SeekOrigin.Begin);
         var message = Encoding.UTF8.GetString(stream.ToArray());
         OnLogEvent?.Invoke(this, new object[] { (int)Logger.MessageType.Server, message.Trim() });
      }

      public void Send(string msg)
      {
         OnLogEvent?.Invoke(this, new object[] { (int)Logger.MessageType.Client, msg.Trim() });
         Send(_socket, msg);
      }

      private void Send(Socket socket, string data)
      {
         var byteData = Encoding.ASCII.GetBytes(data);

         try
         {
            socket.BeginSend(byteData, 0, byteData.Length, 0, out error,
               SendCallback, socket);
         }
         catch (Exception exception)
         {
            if (_socket.IsDisposed()) return;
            OnLogEvent?.Invoke(this, new object[] { (int)Logger.MessageType.Error, exception, OnSendErrorMessage });
            Disconnect();
         }
      }

      private void SendCallback(IAsyncResult ar)
      {
         try
         {
            var socket = (Socket)ar.AsyncState;
            var _ = socket.EndSend(ar);
         }
         catch (Exception e)
         {
            if (_socket.IsDisposed()) return;
            Disconnect();
            OnLogEvent?.Invoke(this, new object[] { (int)Logger.MessageType.Error, e, OnSendErrorMessage });
         }
      }

      public void Disconnect()
      {

         if (IsConnected())
         {
            Disconnect(_socket);
         }
      }

      private void Disconnect(Socket socket)
      {
         try
         {
            socket.Shutdown(SocketShutdown.Both);
            socket.BeginDisconnect(false, DisconnectCallback, socket);
            _done?.WaitOne();
         }
         catch (Exception e)
         {
            if (_socket.IsDisposed()) return;
            OnLogEvent?.Invoke(this, new object[] { (int)Logger.MessageType.Error, e, OnDisconnectErrorMessage });
         }
      }

      private void DisconnectCallback(IAsyncResult ar)
      {
         var socket = (Socket)ar.AsyncState;

         try
         {
            socket.EndDisconnect(ar);
            OnLogEvent?.Invoke(this, new object[] { (int)Logger.MessageType.Success, OnDisconnectSuccessMessage });
         }
         catch (Exception e)
         {
            if (_socket.IsDisposed()) return;
            OnLogEvent?.Invoke(this, new object[] { (int)Logger.MessageType.Error, e, OnDisconnectErrorMessage });
         }

         _done?.Set();
         socket.Close(1000);
      }
   }
}