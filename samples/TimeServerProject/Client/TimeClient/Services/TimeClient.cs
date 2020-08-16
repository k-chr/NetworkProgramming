using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Publishers;
using NetworkingUtilities.Tcp;
using NetworkingUtilities.Udp.Multicast;
using NetworkingUtilities.Utilities;
using NetworkingUtilities.Utilities.Events;

namespace TimeClient.Services
{
	public class TimeClient : ISender, IReceiver, IService
	{
		private readonly List<MulticastClient> _discoveryClients;
		private Client _tcpClient;

		private readonly IReporter _exceptionReporter;
		private readonly IReporter _discoveredServerReporter;
		private readonly IReporter _statusReporter;
		private readonly IReporter _connectedReporter;
		private IReporter _disconnectedReporter;
		private readonly IReporter _timeMessageReporter;

		public TimeClient(string multicastAddress, int multicastPort, int localPort = 0)
		{
			_discoveryClients = GeneralUtilities.GetNetworkInterfacesThatAreUp().ConvertAll(input =>
				new MulticastClient(multicastAddress, multicastPort, localPort: localPort,
					ipAddress: (input.GetIPProperties().UnicastAddresses.SingleOrDefault(ipAddressInformation =>
									ipAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork &&
									!ipAddressInformation.Address.Equals(IPAddress.Loopback))?.Address ??
								IPAddress.Any).ToString()));

			_exceptionReporter = new ExceptionReporter();
			_statusReporter = new StatusReporter();
			_timeMessageReporter = new MessageReporter();
			_connectedReporter = new ClientReporter();
			_disconnectedReporter = new ClientReporter();
			_discoveredServerReporter = new MessageReporter();

			RegisterClients();
		}

		private void RegisterClients() => _discoveryClients.ForEach(RegisterClient);

		private void RegisterClient(AbstractClient client)
		{
		}

		public void Send(byte[] message, string to = "")
		{
			throw new NotImplementedException();
		}

		public void Receive()
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

		private void OnConnect(IPAddress ip, string id, int port) =>
			_connectedReporter.Notify((ip, id, port));

		private void OnDisconnect(IPAddress ip, string id, int port) =>
			_disconnectedReporter.Notify((ip, id, port));

		private void OnException(Exception exception, EventCode code) =>
			_exceptionReporter.Notify((exception, code));

		private void OnStatus(StatusCode code, string statusInfo) =>
			_statusReporter.Notify((code, statusInfo));

		private void OnTimeMessage(byte[] message, string from, string to) =>
			_timeMessageReporter.Notify((message, from, to));

		private void OnDiscoveredServer(byte[] message, string from, string to) =>
			_discoveredServerReporter.Notify((message, from, to));
	}
}