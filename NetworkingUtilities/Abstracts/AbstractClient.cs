using System;
using System.Net;
using System.Net.Sockets;
using NetworkingUtilities.Publishers;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Abstracts
{
	public abstract class AbstractClient : ISender, IReceiver, IService
	{
		protected const int MaxBufferSize = 4096;
		protected readonly Socket ClientSocket;
		private readonly IReporter _lastException;
		private readonly IReporter _lastMessage;
		private readonly IReporter _disconnected;
		protected readonly bool ServerHandler;

		public ClientEvent WhoAmI { get; }

		protected AbstractClient(Socket clientSocket, bool serverHandler = false)
		{
			ClientSocket = clientSocket;
			_lastException = new ExceptionReporter();
			_lastMessage = new MessageReporter();
			_disconnected = new ClientReporter();
			ServerHandler = serverHandler;

			if (ServerHandler)
			{
				if (clientSocket.RemoteEndPoint is IPEndPoint endPoint)
				{
					WhoAmI = new ClientEvent(endPoint.Address, endPoint.Port);
				}
			}
			else
			{
				if (clientSocket.LocalEndPoint is IPEndPoint endPoint)
				{
					WhoAmI = new ClientEvent(endPoint.Address, endPoint.Port);
				}
			}
		}

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

		protected void OnNewMessage(Tuple<string, string, string> messageWithAddresses)
		{
			_lastMessage.Notify(messageWithAddresses);
		}

		protected void OnDisconnect(Tuple<IPAddress, string, int> clientData)
		{
			_disconnected.Notify(clientData);
		}

		protected void OnCaughtException(Exception exception)
		{
			_lastException.Notify(exception);
		}

		public abstract void Send(string message, string to = "");

		public abstract void Receive();

		public abstract void StopService();

		public abstract void StartService();
	}
}