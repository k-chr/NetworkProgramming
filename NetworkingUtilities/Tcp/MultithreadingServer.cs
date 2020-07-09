using System;
using System.Linq;
using System.Net.Sockets;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Tcp
{
	public class MultithreadingServer : AbstractServer
	{
		public MultithreadingServer(string ip, int port, string interfaceName, int maxClientsQueue = 3) : base(ip, port,
			interfaceName)
		{
		}

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
			try
			{
				if (Clients.Any())
				{
					var handler = Clients.First(client => client.WhoAmI.Id.Equals(to));
					handler.Send(message, to);
				}
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception exception)
			{
				OnCaughtException(exception, EventCode.Send);
			}
		}

		public override void StopService()
		{
			DisposeCurrentSession();
		}

		public override void StartService()
		{
			throw new NotImplementedException();
		}
	}
}