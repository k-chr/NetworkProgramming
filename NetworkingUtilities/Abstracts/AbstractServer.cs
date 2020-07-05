using System;
using System.Collections.Generic;
using System.Net;

namespace NetworkingUtilities.Abstracts
{
	public abstract class AbstractServer : ISender, IService
	{
		private readonly IReporter _lastException;
		private readonly IReporter _lastMessage;
		private readonly IReporter _disconnected;
		private readonly IReporter _newClient;

		protected AbstractServer(IReporter disconnected, IReporter lastMessage, IReporter lastException, IReporter newClient)
		{
			_disconnected = disconnected;
			_lastMessage = lastMessage;
			_lastException = lastException;
			_newClient = newClient;

			Clients = new List<AbstractClient>();
		}

		public List<AbstractClient> Clients { get; }

		public abstract void Send(string message, string to = "");

		public abstract void StopService();

		public abstract void StartService();

		protected void AddExceptionSubscription(Action<object, object> procedure)
		{
			_lastException.AddSubscriber(procedure);
		}

		protected void AddMessageSubscription(Action<object, object> procedure)
		{
			_lastMessage.AddSubscriber(procedure);
		}

		protected void AddOnDisconnectedSubscription(Action<object, object> procedure)
		{
			_disconnected.AddSubscriber(procedure);
		}

		public void AddNewClientSubscription(Action<object, object> procedure)
		{
			_newClient.AddSubscriber(procedure);
		}

		public void OnNewMessage(Tuple<string, string, string> messageWithAddresses)
		{
			_lastMessage.Notify(messageWithAddresses);
		}

		public void OnNewClient(Tuple<IPAddress, string, int> clientData)
		{
			_newClient.Notify(clientData);
		}

		public void OnDisconnect(Tuple<IPAddress, string, int> clientData)
		{
			_disconnected.Notify(clientData);
		}

		public void OnCaughtException(Exception exception)
		{
			_lastException.Notify(exception);
		}
	}
}