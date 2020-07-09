using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Tcp
{
	public class IterativeServer : AbstractServer
	{
		public IterativeServer(string ip, int port, string interfaceName) : base(ip, port, interfaceName)
		{
		}

		private void DisposeCurrentSession()
		{
			CleanClients();

			try
			{
				ServerSocket?.Close();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Disconnect);
			}
			catch (Exception e)
			{
				OnCaughtException(e, EventCode.Other);
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
				var endPoint = new IPEndPoint(IPAddress.Parse(Ip), Port);
				ServerSocket.Bind(endPoint);
				ServerSocket.Listen(1);
				OnNewMessage($"Server is currently listening on {endPoint.Address} on {endPoint.Port} port", "server",
					"server");
				AcceptNextPendingConnection();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Connect);
			}
			catch (SecurityException securityException)
			{
				OnCaughtException(securityException, EventCode.Connect);
			}
			catch (Exception e)
			{
				OnCaughtException(e, EventCode.Other);
			}
		}

		private void AcceptNextPendingConnection()
		{
			try
			{
				ServerSocket.BeginAccept(OnAcceptCallback, null);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Accept);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception, EventCode.Other);
			}
		}

		private void OnAcceptCallback(IAsyncResult ar)
		{
			try
			{
				if (ServerSocket is null) throw new ArgumentException("Socket is null");
				var client = ServerSocket.EndAccept(ar);
				var handler = new Client(client, true);
				var whoAreYou = handler.WhoAmI;
				OnNewClient(whoAreYou.Ip, whoAreYou.Id, whoAreYou.Port);
				CleanClients();
				RegisterHandler(handler);
				Clients.Add(handler);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Accept);
			}
			catch (ArgumentException argumentException)
			{
				OnCaughtException(argumentException, EventCode.Accept);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception, EventCode.Other);
			}
		}

		public void AcceptNext() => AcceptNextPendingConnection();

		private void RegisterHandler(AbstractClient handler)
		{
			handler.AddExceptionSubscription((o, o1) =>
			{
				if (o1 is ExceptionEvent e)
					OnCaughtException(e.LastError, e.LastErrorCode);
			});

			handler.AddMessageSubscription((o, o1) =>
			{
				if (o1 is MessageEvent @event)
					OnNewMessage(@event.Message, @event.From, @event.To);
			});

			handler.AddOnDisconnectedSubscription((o, o1) =>
			{
				if (o1 is ClientEvent @event)
				{
					OnDisconnect(@event.Ip, @event.Id, @event.Port);
					Clients.Remove(Clients.FirstOrDefault());
				}
			});
			handler.StartService();
		}

		private void InitSocket()
		{
			try
			{
				ServerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Connect);
			}
			catch (Exception e)
			{
				OnCaughtException(e, EventCode.Other);
			}
		}
	}
}