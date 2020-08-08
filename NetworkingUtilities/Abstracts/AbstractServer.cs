using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using NetworkingUtilities.Publishers;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Abstracts
{
	public abstract class AbstractServer : ISender, IService
	{
		private readonly IReporter _lastException;
		private readonly IReporter _lastMessage;
		private readonly IReporter _statusReporter;
		protected const int MaxBufferSize = 4096;
		protected readonly string Ip;
		protected readonly int Port;
		protected readonly string InterfaceName;
		private readonly IReporter _disconnected;
		private readonly IReporter _newClient;
		protected Socket ServerSocket;

		protected AbstractServer(string ip, int port, string interfaceName)
		{
			Ip = ip;
			Port = port;
			InterfaceName = interfaceName;
			_disconnected = new ClientReporter();
			_lastMessage = new MessageReporter();
			_lastException = new ExceptionReporter();
			_newClient = new ClientReporter();
			_statusReporter = new StatusReporter();
			Clients = new List<AbstractClient>();
		}

		protected ICollection<AbstractClient> Clients { get; }

		public abstract void Send(byte[] message, string to = "");

		public abstract void StopService();

		public abstract void StartService();

		public void AddStatusSubscription(Action<object, object> procedure) => _statusReporter.AddSubscriber(procedure);

		public void AddExceptionSubscription(Action<object, object> procedure) => _lastException.AddSubscriber(procedure);

		public void AddMessageSubscription(Action<object, object> procedure) => _lastMessage.AddSubscriber(procedure);

		public void AddOnDisconnectedSubscription(Action<object, object> procedure) => _disconnected.AddSubscriber(procedure);

		public void AddNewClientSubscription(Action<object, object> procedure) => _newClient.AddSubscriber(procedure);

		protected void OnNewMessage(byte[] message, string from, string to) => _lastMessage.Notify((message, @from, to));

		protected void OnNewClient(IPAddress ip, string id, int port) => _newClient.Notify((ip, id, port));

		protected void OnDisconnect(IPAddress ip, string id, int port) => _disconnected.Notify((ip, id, port));

		protected void OnCaughtException(Exception exception, EventCode code) => _lastException.Notify((exception, code));

		protected void OnReportingStatus(StatusCode code, string msg) => _statusReporter.Notify((code, msg));
	}
}