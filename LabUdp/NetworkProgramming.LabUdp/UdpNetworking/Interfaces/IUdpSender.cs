namespace UdpNetworking.Interfaces
{
   public interface IUdpSender : IUdpService
   {
      void Send(string msg);
   }
}
