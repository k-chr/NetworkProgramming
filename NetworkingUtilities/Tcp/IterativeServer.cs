using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Publishers;

namespace NetworkingUtilities.Tcp
{
	public class IterativeServer : AbstractServer
	{
		private Socket _socket;

		public IterativeServer(string ip, int port, string interfaceName, IReporter disconnected, IReporter lastMessage,
			IReporter lastException, IReporter newClient) : base(ip, port, interfaceName, disconnected, lastMessage,
			lastException, newClient)
		{
		}

		private void DisposeCurrentSession()
		{
			CleanClients();

			try
			{
				_socket?.Close();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException);
			}
		}

		private void CleanClients()
		{
			foreach (var abstractClient in Clients)
			{
				abstractClient.StopService();
			}

			Clients.Clear();
		}

		public override void Send(string message, string to = "")
		{
			if (Clients.Any())
			{
				var handler = Clients.First();
				handler.Send(message, to);
			}
		}

		public override void StopService()
		{
			DisposeCurrentSession();
		}

		public override void StartService()
		{
			DisposeCurrentSession();
			StartListen();
		}

		private void StartListen()
		{
			InitSocket();
			try
			{
				var endPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
				_socket.Bind(endPoint);
				_socket.Listen(1);
				OnNewMessage(new Tuple<string, string, string>(
					$"Server is currently listening on {endPoint.Address} on {endPoint.Port} port", "server", "server"));
				AcceptNextPendingConnection();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException);
			}
			catch (SecurityException securityException)
			{
				OnCaughtException(securityException);
			}
			catch (Exception e)
			{
				OnCaughtException(e);
			}
		}

		private void AcceptNextPendingConnection()
		{
			try
			{
				_socket.BeginAccept(OnAcceptCallback, null);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception);
			}
		}

		private void OnAcceptCallback(IAsyncResult ar)
		{
			try
			{
				if (_socket is null) throw new ArgumentException("Socket is null");
				var client = _socket.EndAccept(ar);
				var handler = new Client(client, new ExceptionReporter(), new MessageReporter(), new ClientReporter());
				var whoAreYou = handler.WhoAmI;
				OnNewClient((whoAreYou.Ip, whoAreYou.Id, whoAreYou.Port).ToTuple());
				CleanClients();
				RegisterHandler(handler);
				Clients.Add(handler);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException);
			}
			catch (ArgumentException argumentException)
			{
				OnCaughtException(argumentException);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception);
			}
		}

		private void RegisterHandler(AbstractClient handler)
		{
			handler.AddExceptionSubscription((o, o1) =>
			{
				if (o1 is Exception e)
				{
					OnCaughtException(e);
				}
			});

			handler.AddMessageSubscription((o, o1) =>
			{
				if (o1 is Tuple<string, string, string> tuple)
					OnNewMessage(tuple);
			});

			handler.AddOnDisconnectedSubscription((o, o1) =>
			{
				if (o1 is Tuple<IPAddress, string, int> tuple)
				{
					OnDisconnect(tuple);
					CleanClients();
					AcceptNextPendingConnection();
				}
			});
		}

		private void InitSocket()
		{
			try
			{
				_socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException);
			}
			catch (Exception e)
			{
				OnCaughtException(e);
			}
		}
	}
}