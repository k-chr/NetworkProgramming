using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Tcp;
using NetworkingUtilities.Udp.Multicast;
using NetworkingUtilities.Utilities;

namespace TimeProjectServices.Services
{
	public class TimeServer : ISender, IService
	{
		private List<MulticastBroadcastServer> _multicastBroadcastServers;
		private List<MultithreadingServer> _tcpServers;

		public TimeServer(string multicastIp, int multicastPort)
		{
			var interfaces = GeneralUtilities.GetNetworkInterfacesThatAreUp().Where(networkInterface =>
				networkInterface.GetIPProperties().UnicastAddresses.All(information =>
					information.Address.AddressFamily == AddressFamily.InterNetwork &&
					!information.Address.Equals(IPAddress.Loopback))).ToList();

			_multicastBroadcastServers = interfaces.ConvertAll(input => new MulticastBroadcastServer(multicastPort,
				multicastIp, input.Name, false, input.GetIPProperties().UnicastAddresses.First().ToString()));
			_tcpServers = interfaces.ConvertAll(input =>
				new MultithreadingServer(input.GetIPProperties().UnicastAddresses.ToString(), 0, input.Name,
					int.MaxValue));
		}

		public void Send(byte[] message, string to = "")
		{
			throw new NotImplementedException();
		}

		public void StopService()
		{
			throw new NotImplementedException();
		}

		public void StartService()
		{
			throw new NotImplementedException();
		}
	}
}