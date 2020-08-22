using System;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Tcp;
using NetworkingUtilities.Udp.Multicast;

namespace TimeProjectServices.Services
{
	public class TimeServer : AbstractServer
	{
		private MulticastBroadcastServer _multicastBroadcastServer;
		private MultithreadingServer _tcpServer;

		public TimeServer(string ip, int port, string interfaceName) : base(ip, port, interfaceName)
		{
		}

		public override void Send(byte[] message, string to = "")
		{
			throw new NotImplementedException();
		}

		public override void StopService()
		{
			throw new NotImplementedException();
		}

		public override void StartService()
		{
			throw new NotImplementedException();
		}
	}
}
