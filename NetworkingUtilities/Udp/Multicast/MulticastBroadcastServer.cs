using NetworkingUtilities.Abstracts;

namespace NetworkingUtilities.Udp.Multicast
{
	class MulticastBroadcastServer : AbstractServer, IReceiver
	{
		public MulticastBroadcastServer(string ip, int port, string interfaceName) : base(ip, port, interfaceName)
		{
		}

		public override void Send(string message, string to = "")
		{
		}

		public override void StopService()
		{
		}

		public override void StartService()
		{
		}

		public void Receive()
		{
		}
	}
}