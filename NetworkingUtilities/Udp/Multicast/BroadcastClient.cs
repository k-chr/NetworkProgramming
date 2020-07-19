using System;
using System.Net;
using System.Net.Sockets;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Extensions;
using static NetworkingUtilities.Utilities.BroadcastTools;
namespace NetworkingUtilities.Udp.Multicast
{
	public class BroadcastClient : AbstractClient
	{
		private readonly int _port;
		private readonly string _ipAddress;
		private readonly int _localPort;
		private IPAddress _address;

		public BroadcastClient(string interfaceIp, int port, bool serverHandler = false,
			string ipAddress = null, int localPort = 0) : base(new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp), serverHandler)
		{
			_port = port;
			_ipAddress = ipAddress;
			_localPort = localPort;
			SetBroadcastIp(interfaceIp);
		}

		public override void Send(string message, string to = "")
		{
			throw new NotImplementedException();
		}

		public override void Receive()
		{
			throw new NotImplementedException();
		}

		public override void StopService() => (ClientSocket == null || ClientSocket.IsDisposed()
			? (Action) (() => { })
			: ClientSocket.Close)();

		public override void StartService()
		{
			throw new NotImplementedException();
		}

		private void SetBroadcastIp(string selectedIp) => _address = GetBroadcastIpForAddress(selectedIp);
	}
}