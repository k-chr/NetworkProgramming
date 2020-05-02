using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UdpNetworking.Services.Enums;
using UdpNetworking.Services.Extensions;
using UdpNetworking.Services.States;

namespace UdpNetworking.Services
{
   public class UdpClientService
   {
      private int _port;
      private string _ip;
      public event EventHandler<object[]> NewMessage;
      public event EventHandler<object[]> NewLog;
      private const int MaxLen = 1024;
      private Socket _socket;
      private EndPoint _endPoint;
      private bool _receiving;

      public void InitializeTransfer(int port, string ip) => InitSocket(port, ip).BeginCommunication();

      public void StopService() => (_socket == null || _socket.IsDisposed() ? (Action)(() => { }) : _socket.Close)();

      private UdpClientService InitSocket(int port, string ip)
      {
         if (_socket != null && !_socket.IsDisposed())
         {
            _socket.Close();
         }
         _port = port;
         _ip = ip;
         _receiving = false;
         var address = IPAddress.Parse(_ip);
         _endPoint = new IPEndPoint(address, _port);
         _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
         return this;
      }

      private void BeginCommunication() => Send("Hi");

      private void Receive(Socket endPoint)
      {
         if(endPoint.IsDisposed()) return;
         var state = new ControlState()
         {
            CurrentSocket = endPoint,
            BufferSize = MaxLen,
            Buffer = new byte[MaxLen],
            StreamBuffer = new MemoryStream()
         };

         NewLog?.Invoke(this, new object[]
         {
            (int)LogLevels.Info, "Started transfer data from sender"
         });

         try
         {
            endPoint.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, 0, ref _endPoint, ReceiveFromCallback, state);
         }
         catch (Exception e)
         {
            NewLog?.Invoke(this, new object[]
            {
               (int)LogLevels.Error, e, "Failed to receive data"
            });
            _receiving = false;
         }

      }

      private void ReceiveFromCallback(IAsyncResult ar)
      {
         try
         {
            if (!(ar.AsyncState is ControlState state)) return;
            var bytesRead = state.CurrentSocket.EndReceiveFrom(ar, ref _endPoint);
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
         catch (Exception e)
         {
            NewLog?.Invoke(this, new object[]
            {
               (int)LogLevels.Error, e, "Failed to receive data"
            });
            _receiving = false;
         }

      }

      private void ProcessMessage(MemoryStream stateStreamBuffer)
      {
         using var stream = stateStreamBuffer;
         stream.Seek(0, SeekOrigin.Begin);
         var message = Encoding.UTF8.GetString(stream.ToArray());
         NewMessage?.Invoke(this, new object[] { (int)LogLevels.Server, message.Trim() });
         NewLog?.Invoke(this, new object[] { (int)LogLevels.Success, "Successfully received message" });
      }

      public void Send(string msg) => Send(_socket, msg);

      private void Send(Socket socket, string msg)
      {
         var data = Encoding.ASCII.GetBytes(msg);
         NewLog?.Invoke(this, new object[]
         {
            (int)LogLevels.Info, "Started transfer data to receiver"
         });
         try
         {
            socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, _endPoint, SendToCallback, socket);
         }
         catch (Exception e)
         {
            NewLog?.Invoke(this, new object[]
            {
               (int)LogLevels.Error, e, "Failed to send data"
            });
         }
      }

      private void SendToCallback(IAsyncResult ar)
      {
         try
         {
            if (!(ar.AsyncState is Socket socket)) return;
            var _ = socket.EndSendTo(ar);
            NewLog?.Invoke(this, new object[]
            {
               (int)LogLevels.Success, "Data were successfully sent"
            });
            if (!_receiving)
            {
               _receiving = true;
               Receive(_socket);
            }
         }
         catch (Exception e)
         {
            NewLog?.Invoke(this, new object[]
            {
               (int)LogLevels.Error, e, "Failed to send data"
            });
         }
      }

   }
}