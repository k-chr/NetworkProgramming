using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using UdpNetworking.Interfaces;
using UdpNetworking.Services.Enums;
using UdpNetworking.Services.Extensions;

namespace UdpNetworking.Services
{
   public class UdpBroadcastClient : IUdpSender
   {
      private Socket _socket;
      private int _port;
      private IPAddress _address;
      public event EventHandler<object[]> LogEvent;

      public void InitSocket(int port)
      {
         LogEvent?.Invoke(this, new object[]
         {
            (int)LogLevels.Info, "Initializing socket"
         });

         try
         {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _port = port;
            _socket.EnableBroadcast = true;
            SetBroadcastIp();
         }
         catch (Exception e)
         {
            LogEvent?.Invoke(this, new object[]
            {
               (int)LogLevels.Error, e, "Couldn't obtain broadcast address"
            });
         }
      }

      public void StopService() => (_socket == null || _socket.IsDisposed() ? (Action)(() => { }) : _socket.Close)();

      private static (IPAddress mask, IPAddress address) ObtainMaskAndLocalIp()
      {
         var ipAddress = Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

         var ipMask = GetSubnetMask(ipAddress);

         return (ipMask, ipAddress);
      }

      private static IPAddress GetSubnetMask(IPAddress address)
      {
         foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
         {
            foreach (var unicastIpAddressInformation in adapter.GetIPProperties().UnicastAddresses)
            {
               if (unicastIpAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork && address.Equals(unicastIpAddressInformation.Address))
               {
                  return unicastIpAddressInformation.IPv4Mask;
               }
            }
         }

         throw new ArgumentException($"Can't find subnet mask for provided IPv4 address '{address}'");
      }

      private void SetBroadcastIp()
      {
         var (mask, address) = ObtainMaskAndLocalIp();
         var ipAddress = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
         var ipMaskV4 = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);
         var broadCastIpAddress = ipAddress | ~ipMaskV4;

         _address = new IPAddress(BitConverter.GetBytes(broadCastIpAddress));
      }

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
               (int)LogLevels.Success, "Data were successfully broadcast"
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
