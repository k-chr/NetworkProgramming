using System;
using System.Net;
using System.Net.Sockets;
using NetworkingUtilities.Abstracts;

namespace NetworkingUtilities.Udp.Unicast
{
	class UnicastClient : AbstractClient
	{
		private IPEndPoint _endPoint;
		private bool _listening;

		public UnicastClient(IPEndPoint endPoint, Socket clientSocket, bool serverHandler = false) : base(clientSocket, serverHandler)
		{
			_endPoint = endPoint;
		}

		public UnicastClient(string ip, int port) : base(new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
			ProtocolType.Udp))
		{
			var address = IPAddress.Parse(ip);
			_endPoint = new IPEndPoint(address, port);
		}

		public override void Send(string message, string to = "")
		{
			throw new NotImplementedException();
		}

		public override void Receive()
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