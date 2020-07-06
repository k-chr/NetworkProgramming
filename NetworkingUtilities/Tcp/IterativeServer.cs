using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Security;
using NetworkingUtilities.Abstracts;

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
			foreach (var abstractClient in Clients)
			{
				abstractClient.StopService();
			}

			Clients.Clear();

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

		public override void Send(string message, string to = "")
		{
			throw new NotImplementedException();
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