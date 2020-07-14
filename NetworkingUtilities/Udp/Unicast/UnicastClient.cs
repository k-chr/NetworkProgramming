using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Udp.Unicast
{
	class UnicastClient : AbstractClient
	{
		private IPEndPoint _endPoint;
		private bool _listening;

		public UnicastClient(IPEndPoint endPoint, Socket clientSocket, bool serverHandler = false) : base(clientSocket,
			serverHandler)
		{
			_endPoint = endPoint;
		}

		public UnicastClient(string ip, int port) : base(new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
			ProtocolType.Udp))
		{
			var address = IPAddress.Parse(ip);
			_endPoint = new IPEndPoint(address, port);
		}

		public override void Send(string message, string to = "")
		{
			if (!string.IsNullOrEmpty(to))
			{
				var strings = to.Split(':');
				try
				{
					var ipAddress = IPAddress.Parse(strings[0]);
					var port = int.Parse(strings[1]);
					_endPoint = new IPEndPoint(ipAddress, port);
				}
				catch (Exception e)
				{
					OnCaughtException(e, EventCode.Other);
					return;
				}
			}

			var data = Encoding.ASCII.GetBytes(message);

			try
			{
				ClientSocket.BeginSendTo(data, 0, data.Length, 0, _endPoint, OnSendToCallback, ClientSocket);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Send);
			}
			catch (Exception e)
			{
				OnCaughtException(e, EventCode.Other);
			}
		}

		private void OnSendToCallback(IAsyncResult ar)
		{
		}

		public override void Receive()
		{
			throw new NotImplementedException();
		}

		public override void StopService()
		{
			throw new NotImplementedException();
		}

		public override void StartService()
		{
			throw new NotImplementedException();
		}
	}
}