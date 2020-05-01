using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UdpNetworking.Services.Enums;
using UdpNetworking.Services.Extensions;
using UdpNetworking.Services.States;

namespace UdpNetworking.Services
{
   public class UdpMulticastBroadcastReceiver
   {
      private Socket _serverSocket;
      private int _port;
      private const int MaxLen = 1024;
      private bool _acceptBroadcast;
      private IPAddress _address;
      private readonly Dictionary<EndPoint, ControlState> _clientsBuffers;

      public event EventHandler<object[]> LogEvent;

      public UdpMulticastBroadcastReceiver() => _clientsBuffers = new Dictionary<EndPoint, ControlState>();

      public void InitSocket(string groupAddress, int port, bool acceptBroadcast)
      {
         LogEvent?.Invoke(this, new object[]
         {
            (int)LogLevels.Info, "Initializing socket"
         });

         try
         {
            _port = port;
            _acceptBroadcast = acceptBroadcast;
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
               EnableBroadcast = _acceptBroadcast
            };

            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, _port));
            var add = IPAddress.Parse(groupAddress);

            _serverSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, 1);
            _serverSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 255);
            _serverSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(add));

            _address = add;
         }
         catch (Exception e)
         {
            LogEvent?.Invoke(this, new object[]
            {
               (int)LogLevels.Error, e, "Cannot init socket properly"
            });
         }
      }

      public void StopService() => (_serverSocket == null || _serverSocket.IsDisposed() ? (Action)(() => { }) : () =>
      {
         _serverSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, _address);
         _serverSocket.Close();
      })();

      public void Receive() => Receive(_serverSocket);

      private void Receive(Socket socket)
      {
         if (socket.IsDisposed()) return;
         var state = new ControlState()
         {
            CurrentSocket = socket,
            BufferSize = MaxLen,
            Buffer = new byte[MaxLen],
            StreamBuffer = new MemoryStream()
         };

         LogEvent?.Invoke(this, new object[]
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
            LogEvent?.Invoke(this, new object[]
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
            else if (_clientsBuffers[end].StreamBuffer.CanWrite && _clientsBuffers[end].StreamBuffer.Length > 0)
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
            LogEvent?.Invoke(this, new object[]
            {
               (int)LogLevels.Error, e, "Failed to receive data"
            });
         }


      }

      private void ProcessMessage(EndPoint end)
      {
         if (!_clientsBuffers.ContainsKey(end)) return;
         var state = _clientsBuffers[end];
         using var stream = state.StreamBuffer;
         stream.Seek(0, SeekOrigin.Begin);
         var message = Encoding.UTF8.GetString(stream.ToArray());
         LogEvent?.Invoke(this, new object[] { (int)LogLevels.Success, "Successfully received message" });
         LogEvent?.Invoke(this, new object[] { (int)LogLevels.Client, end, message });
      }
   }
}
