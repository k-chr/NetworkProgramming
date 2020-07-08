using System;
using System.Collections.Generic;
using System.Net;
using NetworkingUtilities.Publishers;

namespace NetworkingUtilities.Abstracts
{
	public abstract class AbstractServer : ISender, IService
	{
		private readonly IReporter _lastException;
		private readonly IReporter _lastMessage;
		protected readonly string Ip;
		protected readonly int Port;
		protected readonly string InterfaceName;
		private readonly IReporter _disconnected;
		private readonly IReporter _newClient;

		protected AbstractServer(string ip, int port, string interfaceName)
		{
			Ip = ip;
			Port = port;
			InterfaceName = interfaceName;
			_disconnected = new ClientReporter();
			_lastMessage = new MessageReporter();
			_lastException = new ExceptionReporter();
			_newClient = new ClientReporter();
			Clients = new List<AbstractClient>();
		}

		public ICollection<AbstractClient> Clients { get; }

		public abstract void Send(string message, string to = "");

		public abstract void StopService();

		public abstract void StartService();

		public void AddExceptionSubscription(Action<object, object> procedure)
		{
			_lastException.AddSubscriber(procedure);
		}

		public void AddMessageSubscription(Action<object, object> procedure)
		{
			_lastMessage.AddSubscriber(procedure);
		}

		public void AddOnDisconnectedSubscription(Action<object, object> procedure)
		{
			_disconnected.AddSubscriber(procedure);
		}

		public void AddNewClientSubscription(Action<object, object> procedure)
		{
			_newClient.AddSubscriber(procedure);
		}

		protected void OnNewMessage(Tuple<string, string, string> messageWithAddresses)
		{
			_lastMessage.Notify(messageWithAddresses);
		}

		protected void OnNewClient(Tuple<IPAddress, string, int> clientData)
		{
			_newClient.Notify(clientData);
		}

		protected void OnDisconnect(Tuple<IPAddress, string, int> clientData)
		{
			_disconnected.Notify(clientData);
		}

		protected void OnCaughtException(Exception exception)
		{
			_lastException.Notify(exception);
		}
	}
}