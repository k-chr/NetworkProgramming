using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetworkProgramming.Lab1
{
   public class ControlState
   {
      public Socket CurrentSocket = null;
      public long BufferSize { get; set; }
      public byte[] Buffer = null;
      public MemoryStream StreamBuffer = new MemoryStream();
   }

   public class Client
   {
      private readonly int _max_len = 1024;
      
      private readonly ManualResetEvent _done = new ManualResetEvent(false);
      private readonly Socket _socket = null;

      public Client(string address, int port, ManualResetEvent manualResetEvent)
      {
         _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
         _done = manualResetEvent;
         try
         {
            _socket.BeginConnect(address, port, OnConnectedCallback, _socket);
         }
         catch (Exception e)
         {
            Logger.LogError(e, "Failed to connect to remote server\n");
            Disconnect();
         }
      }

      public bool IsConnected()
      {
         return _socket.Connected;
      }

      private void OnConnectedCallback(IAsyncResult ar)
      {
         try
         {
            var socket = (Socket) ar.AsyncState;
            socket.EndConnect(ar);
            Receive(socket);
            Logger.LogSuccess("Succeeded in connecting to remote server\n");
            _done.Set();
         }
         catch (Exception e)
         {
            Logger.LogError(e, "Failed to connect to remote server\n");
            Disconnect();
         }
      }

      private void Receive(Socket socket)
      {
         var state = new ControlState
         {
            CurrentSocket = socket,
            Buffer = new byte[_max_len],
            BufferSize = _max_len
         };

         try
         {
            socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0,
               ReceiveCallback, state);
         }
         catch (Exception exception)
         {
            Logger.LogError(exception, "Failed to send message due to connection issues\n");
            Disconnect();
         }
      }

      private void ReceiveCallback(IAsyncResult ar)
      {
         var state = (ControlState)ar.AsyncState;
         var client = state.CurrentSocket;
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

         state.Buffer = new byte[state.BufferSize];

         try
         {
            client.BeginReceive(state.Buffer, 0, (int)state.BufferSize, 0, ReceiveCallback, state);
         }
         catch (Exception exception)
         {
            if (_socket.Connected)
            {
               Logger.LogError(exception, "Failed to receive message due to connection issues\n");
            }

            Disconnect();
         }

      }

      private void ProcessMessage(MemoryStream memory)
      {
         using var stream = memory;
         stream.Seek(0, SeekOrigin.Begin);
         var message = Encoding.UTF8.GetString(stream.ToArray());
         Logger.LogMsg(message.Trim(), true);
      }

      public void Send(string msg)
      {
         Logger.LogMsg(msg);
         Send(_socket, msg);
      }

      private void Send(Socket socket, string data)
      {
         var byteData = Encoding.ASCII.GetBytes(data);

         try
         {
            socket.BeginSend(byteData, 0, byteData.Length, 0,
               SendCallback, socket);
         }
         catch (Exception exception)
         {
            Logger.LogError(exception, "Cannot send provided message to server due to connection issues!\n");
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
            Logger.LogError(e, "Cannot send provided message to server due to connection issues!\n");
            Disconnect();
         }
      }

      public void Disconnect()
      {
         if (_socket.Connected)
         {
            Disconnect(_socket);
         }
      }

      private void Disconnect(Socket socket)
      {
         try
         {
            socket.Shutdown(SocketShutdown.Both);
            socket.BeginDisconnect(true, DisconnectCallback, socket);
            _done.WaitOne();
         }
         catch (Exception e)
         {
            Logger.LogError(e, "Can't properly disconnect from host\n");
            Logger.LogInfo("Shutting down\n");
            Console.ReadKey();
            Environment.Exit(1);
         }
      }

      private void DisconnectCallback(IAsyncResult ar)
      {
         var socket = (Socket)ar.AsyncState;

         try
         {
            socket.EndDisconnect(ar);
            Logger.LogSuccess("Successfully disconnected\n");
         }
         catch (Exception e)
         {
            Logger.LogError(e, "Can't properly disconnect from host\n");
            Logger.LogInfo("Shutting down\n");
            Console.ReadKey();
            Environment.Exit(1);
         }

         _done.Set();
      }
   }
}