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
using TimeProjectServices.Enums;
using TimeProjectServices.Protocols;

namespace TimeProjectServices.Services
{
	public class TimeServer : IService
	{
		private readonly int _multicastPort;
		private readonly List<MulticastBroadcastServer> _multicastBroadcastServers;
		private readonly List<MultithreadingServer> _tcpServers;

		private readonly IReporter _exceptionReporter;
		private readonly IReporter _discoverServerRequestReporter;
		private readonly IReporter _statusReporter;
		private readonly IReporter _newClientReporter;
		private readonly IReporter _clientDisconnectedReporter;
		private readonly IReporter _timeMessageRequestReporter;

		public TimeServer(string multicastIp, int multicastPort)
		{
			_multicastPort = multicastPort;
			var interfaces = GeneralUtilities.GetNetworkInterfacesThatAreUp().Where(networkInterface =>
				networkInterface.GetIPProperties().UnicastAddresses.All(information =>
					information.Address.AddressFamily == AddressFamily.InterNetwork &&
					!information.Address.Equals(IPAddress.Loopback))).ToList();

			_multicastBroadcastServers = interfaces.ConvertAll(input => new MulticastBroadcastServer(multicastPort,
				multicastIp, input.Name, false, input.GetIPProperties().UnicastAddresses.First().ToString()));
			_tcpServers = interfaces.ConvertAll(input =>
				new MultithreadingServer(input.GetIPProperties().UnicastAddresses.ToString(),
					LocalIdSupplier.CreatePort(), input.Name,
					int.MaxValue));

			_exceptionReporter = new ExceptionReporter();
			_discoverServerRequestReporter = new MessageReporter();
			_statusReporter = new StatusReporter();
			_newClientReporter = new ClientReporter();
			_clientDisconnectedReporter = new ClientReporter();
			_timeMessageRequestReporter = new MessageReporter();

			RegisterServers();
		}

		private void RegisterServers()
		{
			_tcpServers.ForEach(RegisterServer);
			_multicastBroadcastServers.ForEach(RegisterServer);
		}

		public void SendProtocol(IProtocol protocol, IPEndPoint from, string to)
		{
			if (protocol.Action != ActionType.Response) return;
			var bytes = protocol.GetBytes();
			AbstractServer server = protocol.Header switch
									{
										HeaderType.Discover => _multicastBroadcastServers.First(multicastServer =>
											multicastServer.EndPoint.Equals(from)),
										HeaderType.Time => _tcpServers.First(multithreadingServer =>
											multithreadingServer.EndPoint.Equals(from)),
										_ => throw new ArgumentOutOfRangeException()
									};

			server.Send(bytes, to);
		}

		private void RegisterServer(MulticastBroadcastServer server)
		{
			server.AddExceptionSubscription((o, o1) =>
			{
				if (o1 is ExceptionEvent exceptionEvent)
					OnNewException(exceptionEvent.LastError, exceptionEvent.LastErrorCode);
			});

			server.AddMessageSubscription((o, o1) =>
			{
				if (o1 is MessageEvent messageEvent)
					OnDiscoverRequest(messageEvent.Message, messageEvent.From, server.EndPoint.ToString());
			});

			server.AddStatusSubscription((o, o1) =>
			{
				if (o1 is StatusEvent statusEvent)
					OnStatus(statusEvent.StatusCode, statusEvent.StatusMessage);
			});
		}

		private void RegisterServer(MultithreadingServer server)
		{
			server.AddExceptionSubscription((o, o1) =>
			{
				if (o1 is ExceptionEvent exceptionEvent)
				{
					OnNewException(exceptionEvent.LastError, exceptionEvent.LastErrorCode);
					if (exceptionEvent.LastErrorCode == EventCode.Bind)
					{
						do
						{
							server.Port = LocalIdSupplier.CreatePort();
						} while (server.Port == _multicastPort);

						server.StartService();
					}
				}
			});

			server.AddMessageSubscription((o, o1) =>
			{
				if (o1 is MessageEvent messageEvent)
					OnTimeMessageRequest(messageEvent.Message, messageEvent.From, server.EndPoint.ToString());
			});

			server.AddNewClientSubscription((o, o1) =>
			{
				if (o1 is ClientEvent clientEvent)
					OnNewClient(clientEvent.Ip, clientEvent.Id, clientEvent.Port);
			});

			server.AddOnDisconnectedSubscription((o, o1) =>
			{
				if (o1 is ClientEvent clientEvent)
					OnClientDisconnected(clientEvent.Ip, clientEvent.Id, clientEvent.Port);
			});

			server.AddStatusSubscription((o, o1) =>
			{
				if (o1 is StatusEvent statusEvent)
					OnStatus(statusEvent.StatusCode, statusEvent.StatusMessage);
			});
		}

		public void AddExceptionSubscription(Action<object, object> procedure) =>
			_exceptionReporter.AddSubscriber(procedure);

		public void AddDiscoverRequestSubscription(Action<object, object> procedure) =>
			_discoverServerRequestReporter.AddSubscriber(procedure);

		public void AddOnNewClientSubscription(Action<object, object> procedure) =>
			_newClientReporter.AddSubscriber(procedure);

		public void AddStatusSubscription(Action<object, object> procedure) =>
			_statusReporter.AddSubscriber(procedure);

		public void AddTimeMessageRequestSubscription(Action<object, object> procedure) =>
			_timeMessageRequestReporter.AddSubscriber(procedure);

		public void AddOnDisconnectionSubscription(Action<object, object> procedure) =>
			_clientDisconnectedReporter.AddSubscriber(procedure);

		private void OnNewClient(IPAddress address, string id, int port) =>
			_newClientReporter.Notify((address, id, port));

		private void OnNewException(Exception exception, EventCode code) =>
			_exceptionReporter.Notify((exception, code));

		private void OnStatus(StatusCode code, string info) =>
			_statusReporter.Notify((code, info));

		private void OnClientDisconnected(IPAddress address, string id, int port) =>
			_clientDisconnectedReporter.Notify((address, id, port));

		private void OnTimeMessageRequest(byte[] message, string from, string to) =>
			_timeMessageRequestReporter.Notify((message, from, to));

		private void OnDiscoverRequest(byte[] message, string from, string to) =>
			_discoverServerRequestReporter.Notify((message, from, to));

		public void StopService()
		{
			OnStatus(StatusCode.Info, "Stopping internal services of TimeServer");
			_tcpServers.ForEach(server => server.StopService());
			_multicastBroadcastServers.ForEach(server => server.StopService());
		}

		public void StartService()
		{
			OnStatus(StatusCode.Info, "Starting internal services of TimeServer");
			_tcpServers.ForEach(server => server.StartService());
			_multicastBroadcastServers.ForEach(server => server.StartService());
		}
	}
}