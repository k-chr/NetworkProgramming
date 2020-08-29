using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Publishers;
using NetworkingUtilities.Tcp;
using NetworkingUtilities.Udp.Multicast;
using NetworkingUtilities.Utilities;
using NetworkingUtilities.Utilities.Events;
using TimeProjectServices.Enums;
using TimeProjectServices.Protocols;

namespace TimeProjectServices.Services
{
	public class TimeClient : IService
	{
		private readonly List<MulticastClient> _discoveryClients;
		private Client _tcpClient;

		private readonly IReporter _exceptionReporter;
		private readonly IReporter _discoveredServerReporter;
		private readonly IReporter _statusReporter;
		private readonly IReporter _connectedReporter;
		private readonly IReporter _disconnectedReporter;
		private readonly IReporter _timeMessageReporter;
		private readonly ManualResetEvent _clientManualEvent = new ManualResetEvent(false);

		public TimeClient(string multicastAddress, int multicastPort, int localPort = 0)
		{
			_discoveryClients = GeneralUtilities.GetNetworkInterfacesThatAreUp().Where(networkInterface =>
					networkInterface.GetIPProperties().UnicastAddresses
					   .All(information => !information.Address.Equals(IPAddress.Loopback)))
			   .ToList().ConvertAll(input => new MulticastClient(multicastAddress, multicastPort, localPort: localPort,
					ipAddress: input.GetIPProperties().UnicastAddresses.First(ipAddressInformation =>
						ipAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork).Address.ToString()));

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
			client.AddExceptionSubscription((o, o1) =>
			{
				if (o1 is ExceptionEvent e)
					OnException(e.LastError, e.LastErrorCode);
			});

			client.AddOnDisconnectedSubscription((o, o1) =>
			{
				if (o1 is ClientEvent clientEvent)
					OnDisconnect(clientEvent.Id, clientEvent.Ip, clientEvent.ServerIp);
			});

			client.AddOnConnectedSubscription((o, o1) =>
			{
				if (o1 is ClientEvent clientEvent)
					OnConnect(clientEvent.Id, clientEvent.Ip, clientEvent.ServerIp);
			});

			client.AddStatusSubscription((o, o1) =>
			{
				if (o1 is StatusEvent statusEvent)
					OnStatus(statusEvent.StatusCode, statusEvent.StatusMessage);
			});

			switch (client)
			{
				case MulticastClient _:
					client.AddMessageSubscription((o, o1) =>
					{
						if (o1 is MessageEvent message)
							OnDiscoveredServer(message.Message, message.From, message.To);
					});
					break;
				case Client _:
					client.AddMessageSubscription((o, o1) =>
					{
						if (o1 is MessageEvent message)
							OnTimeMessage(message.Message, message.From, message.To);
					});
					break;
			}
		}

		public void SendProtocol(IProtocol protocol, string to)
		{
			var message = protocol.GetBytes();
			switch (protocol.Header)
			{
				case HeaderType.Time:
					_tcpClient?.Send(message);
					break;
				case HeaderType.Discover:
					_discoveryClients.ForEach(client => client.Send(message, to));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void StopService()
		{
			_discoveryClients?.ForEach(client => client.StopService());
			StopTimeCommunication();
			OnStatus(StatusCode.Info, "Stopped internal modules of TimeClient");
		}

		public void StartService() => _discoveryClients.ForEach(client => client.StartService());

		public void StopTimeCommunication()
		{
			if (_tcpClient != null)
			{
				_tcpClient.StopService();
			}
			else
			{
				OnDisconnect("", new IPEndPoint(IPAddress.Any, 0), new IPEndPoint(IPAddress.Any, 0));
			}

			_tcpClient = null;
		}

		public void StartTimeCommunication(IPEndPoint endPoint)
		{
			var address = endPoint?.Address.ToString();
			var port = endPoint?.Port ?? 0;
			_tcpClient = new Client();
			RegisterClient(_tcpClient);

			Task.Run(() =>
			{
				_tcpClient.Connect(address, port, _clientManualEvent);
				_clientManualEvent.WaitOne();
				_tcpClient.StartService();
			});
		}

		public void AddExceptionSubscription(Action<object, object> procedure) =>
			_exceptionReporter.AddSubscriber(procedure);

		public void AddTimeMessageSubscription(Action<object, object> procedure) =>
			_timeMessageReporter.AddSubscriber(procedure);

		public void AddOnDisconnectedSubscription(Action<object, object> procedure) =>
			_disconnectedReporter.AddSubscriber(procedure);

		public void AddOnConnectedSubscription(Action<object, object> procedure) =>
			_connectedReporter.AddSubscriber(procedure);

		public void AddStatusSubscription(Action<object, object> procedure) =>
			_statusReporter.AddSubscriber(procedure);

		public void AddDiscoveredServerSubscription(Action<object, object> procedure) =>
			_discoveredServerReporter.AddSubscriber(procedure);

		private void OnConnect(string id, IPEndPoint ip, IPEndPoint serverIp) =>
			_connectedReporter.Notify((id, ip, serverIp));

		private void OnDisconnect(string id, IPEndPoint ip, IPEndPoint serverIp) =>
			_disconnectedReporter.Notify((id, ip, serverIp));

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