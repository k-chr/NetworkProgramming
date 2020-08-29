using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Tcp
{
	public class MultithreadingServer : AbstractServer
	{
		private readonly int _maxClientsQueue;

		public MultithreadingServer(string ip, int port, string interfaceName, int maxClientsQueue = 3) : base(ip, port,
			interfaceName)
		{
			_maxClientsQueue = maxClientsQueue;
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
					Task.Run(() =>
					{
						lock (Lock)
						{
							Clients.Remove(Clients.FirstOrDefault(client => client.WhoAmI.Equals(handler.WhoAmI)));
							OnDisconnect(@event.Id, @event.Ip, @event.ServerIp);
						}
					});
				}
			});

			handler.AddStatusSubscription((o, o1) =>
			{
				if (o1 is StatusEvent @event)
					OnReportingStatus(@event.StatusCode, @event.StatusMessage);
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
			lock (Lock)
			{
				foreach (var abstractClient in Clients)
				{
					abstractClient.StopService();
				}
			}

			Clients.Clear();
		}

		public override void Send(byte[] message, string to = "")
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
				OnReportingStatus(StatusCode.Success, $"Successfully bound to {endPoint}");
				ServerSocket.Listen(1);
				OnReportingStatus(StatusCode.Info, $"Started iterative TCP listening on {endPoint}");
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
				OnReportingStatus(StatusCode.Info, "Started accepting new TCP connection");
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
				OnReportingStatus(StatusCode.Success, $"Successfully accepted new TCP connection");

				if (Clients.Count >= _maxClientsQueue)
				{
					OnReportingStatus(StatusCode.Error,
						$"Rejected next pending TCP connection. Accepted clients count: {Clients.Count} >= Maximum count of clients: {_maxClientsQueue}");
					client.Send(Encoding.UTF8.GetBytes("Rejected connection"));
					client.Shutdown(SocketShutdown.Both);
					client.Close(1000);
				}
				else
				{
					var handler = new Client(client, true);
					var whoAreYou = handler.WhoAmI;
					OnNewClient(whoAreYou.Id, whoAreYou.Ip, whoAreYou.ServerIp);
					RegisterHandler(handler);
					lock (Lock)
					{
						Clients.Add(handler);
					}
				}

				AcceptNextPendingConnection();
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

		private void InitSocket()
		{
			try
			{
				EndPoint = null;
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