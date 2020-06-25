using System;
using System.Collections.Generic;
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
   public class UdpServerService
   {
      private int _port;
      private string _ip;
      private Socket _serverSocket;
      public event EventHandler<object[]> NewMessage;
      public event EventHandler<object[]> NewLog;
      private const int MaxLen = 1024;
      private EndPoint _localEndPoint;
      private readonly Dictionary<EndPoint, ControlState> _clientsBuffers;

      public UdpServerService()
      {
         _clientsBuffers = new Dictionary<EndPoint, ControlState>();
      }

      private UdpServerService InitSocket(int port, string ip)
      {
         _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
         _port = port;
         _ip = ip;
         var address = IPAddress.Parse(_ip);
         var endPoint = new IPEndPoint(address, _port);
         _localEndPoint = endPoint;
         _serverSocket.Bind(_localEndPoint);
         return this;
      }

      public void InitializeTransfer(int port, string ip) => InitSocket(port, ip).Receive(_serverSocket);

      public void StopService() => (_serverSocket == null || _serverSocket.IsDisposed() ? (Action)(() => { }) : _serverSocket.Close)();

      private void Receive(Socket socket)
      {
         if(socket.IsDisposed()) return;
         var state = new ControlState()
         {
            CurrentSocket = socket,
            BufferSize = MaxLen,
            Buffer = new byte[MaxLen],
            StreamBuffer = new MemoryStream()
         };

         NewLog?.Invoke(this, new object[]
         {
            (int)LogLevels.Info, "Started transfer data from potential senders"
         });

         try
         {
            var endPoint = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
            socket.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, 0, ref endPoint, ReceiveFromCallback, state);
         }
         catch (Exception e)
         {
            NewLog?.Invoke(this, new object[]
            {
               (int)LogLevels.Error, e, "Failed to receive data"
            });
         }
      }

      private void ReceiveFromCallback(IAsyncResult ar)
      {
         try
         {
            var end = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
            if (!(ar.AsyncState is ControlState state)) return;
            var bytesRead = state.CurrentSocket.EndReceiveFrom(ar, ref end);

            if (!_clientsBuffers.ContainsKey(end))
            {
               var s = new ControlState
               {
                  Buffer = new byte[MaxLen],
                  BufferSize = MaxLen,
                  StreamBuffer = new MemoryStream(),
               };
               _clientsBuffers.Add(end, s);
            }

            if (bytesRead > 0)
            {
               _clientsBuffers[end].StreamBuffer.Write(state.Buffer, 0, bytesRead);
               if (state.Buffer.Any(byte_ => byte_ == '\0'))
               {
                  ProcessMessage(end);
                  _clientsBuffers[end].StreamBuffer = new MemoryStream();
               } 
            }
            else if(_clientsBuffers[end].StreamBuffer.CanWrite && _clientsBuffers[end].StreamBuffer.Length > 0)
            {
               ProcessMessage(end);
               _clientsBuffers[end].StreamBuffer = new MemoryStream();
            }

            var e = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
            state.CurrentSocket.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ref e,
               ReceiveFromCallback, state);
         }
         catch (Exception e)
         {
            NewLog?.Invoke(this, new object[]
            {
               (int)LogLevels.Error, e, "Failed to receive data"
            });
         }
      }

      private void ProcessMessage(EndPoint end)
      {
         if(!_clientsBuffers.ContainsKey(end)) return;
         var state = _clientsBuffers[end];
         using var stream = state.StreamBuffer;
         stream.Seek(0, SeekOrigin.Begin);
         var message = Encoding.UTF8.GetString(stream.ToArray());
         NewMessage?.Invoke(this, new object[] { (int)LogLevels.Client, message.Trim(), end as IPEndPoint });
         NewLog?.Invoke(this, new object[] { (int)LogLevels.Success, "Successfully received message" });
      }

      public void Send(string message, string ip, int port) => Send(_serverSocket, message, ip, port);

      private void Send(Socket serverSocket, string message, string ip, int port)
      {
         var data = Encoding.ASCII.GetBytes(message);
         NewLog?.Invoke(this, new object[]
         {
            (int)LogLevels.Info, "Started transfer data to receiver"
         });

         try
         {
            var endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            var state = new ReceiverState
            {
               Port = port,
               Socket = serverSocket,
               Ip = ip
            };
            serverSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, endPoint, SendToCallback, state);
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
         if (!(ar.AsyncState is ReceiverState state)) return;
         try
         {
            var _ = state.Socket.EndSendTo(ar);
            NewLog?.Invoke(this, new object[]
            {
               (int)LogLevels.Success, $"Data were successfully sent to {state.Ip}:{state.Port}"
            });
         }
         catch (Exception e)
         {
            NewLog?.Invoke(this, new object[]
            {
               (int)LogLevels.Error, e, $"Failed to send data to {state.Ip}:{state.Port}"
            });
         }
      }
   }
}
