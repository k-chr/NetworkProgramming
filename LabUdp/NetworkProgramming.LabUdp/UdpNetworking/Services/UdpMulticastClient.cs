using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UdpNetworking.Interfaces;
using UdpNetworking.Services.Enums;
using UdpNetworking.Services.Extensions;

namespace UdpNetworking.Services
{
   public class UdpMulticastClient : IUdpSender
   {
      private Socket _socket;
      private int _port;
      private IPAddress _address;
      public event EventHandler<object[]> LogEvent;

      public void InitSocket(string address, int port)
      {
         LogEvent?.Invoke(this, new object[]
         {
            (int)LogLevels.Info, "Initializing socket"
         });

         try
         {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _port = port;
            var add = IPAddress.Parse(address);

            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, 1);
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 255);
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(add));

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

      public void StopService() => (_socket == null || _socket.IsDisposed() ? (Action)(() => { }) : () =>
      {
         _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(_address));
         _socket.Close();
      })();

      public void Send(string msg) => Send(_socket, msg);

      private void Send(Socket socket, string msg)
      {
         if (socket.IsDisposed()) return;
         LogEvent?.Invoke(this, new object[]
         {
            (int)LogLevels.Info, "Started broadcast transfer data to receivers"
         });

         var data = Encoding.ASCII.GetBytes(msg);

         try
         {
            var endPoint = new IPEndPoint(_address, _port);
            socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, endPoint, SendToCallback, socket);
         }
         catch (Exception e)
         {
            LogEvent?.Invoke(this, new object[]
            {
               (int)LogLevels.Error, e, "Failed to send data"
            });
         }
      }

      private void SendToCallback(IAsyncResult ar)
      {
         if (!(ar.AsyncState is Socket socket)) return;

         try
         {
            var _ = socket.EndSendTo(ar);
            LogEvent?.Invoke(this, new object[]
            {
               (int)LogLevels.Success, "Data were successfully multicast"
            });
         }
         catch (Exception e)
         {
            LogEvent?.Invoke(this, new object[]
            {
               (int)LogLevels.Error, e, "Failed to send data"
            });
         }
      }
   }
}
