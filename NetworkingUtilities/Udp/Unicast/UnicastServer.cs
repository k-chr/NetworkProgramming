using System;
using NetworkingUtilities.Abstracts;

namespace NetworkingUtilities.Udp.Unicast
{
	public class UnicastServer : AbstractServer
	{
		public UnicastServer(string ip, int port, string interfaceName) : base(ip, port, interfaceName)
		{
		}

		public override void Send(string message, string to = "")
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
