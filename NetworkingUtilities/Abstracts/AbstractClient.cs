using System;
using System.Net;
using System.Net.Sockets;
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

		protected AbstractClient(Socket clientSocket, IReporter lastException, IReporter lastMessage,
			IReporter disconnected, bool serverHandler = false)
		{
			ClientSocket = clientSocket;
			_lastException = lastException;
			_lastMessage = lastMessage;
			_disconnected = disconnected;
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

		public void OnNewMessage(Tuple<string, string, string> messageWithAddresses)
		{
			_lastMessage.Notify(messageWithAddresses);
		}

		public void OnDisconnect(Tuple<IPAddress, string, int> clientData)
		{
			_disconnected.Notify(clientData);
		}

		public void OnCaughtException(Exception exception)
		{
			_lastException.Notify(exception);
		}

		public abstract void Send(string message, string to = "");

		public abstract void Receive();

		public abstract void StopService();

		public abstract void StartService();
	}
}