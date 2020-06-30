using System;

namespace UdpNetworking.Interfaces
{
   public interface IUdpService
   {
      event EventHandler<object[]> LogEvent;
      void StopService();
   }
}
