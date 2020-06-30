using System.Net.Sockets;

namespace UdpNetworking.Services.States
{
   public class ReceiverState
   {
      public Socket Socket;
      public string Ip;
      public int Port;
   }
}
