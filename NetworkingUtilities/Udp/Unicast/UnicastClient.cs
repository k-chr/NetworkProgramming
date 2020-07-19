using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Extensions;
using NetworkingUtilities.Utilities.Events;
using NetworkingUtilities.Utilities.StateObjects;
using EndPoint = System.Net.EndPoint;
using Exception = System.Exception;

namespace NetworkingUtilities.Udp.Unicast
{
	public class UnicastClient : AbstractClient
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
					_listening = false;
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
			try
			{
				if (ar.AsyncState is Socket socket)
				{
					var _ = socket.EndSendTo(ar);
					if (!_listening)
					{
						Receive();
						_listening = true;
					}
				}
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

		public override void Receive()
		{
			try
			{
				var state = new ControlState
				{
					CurrentSocket = ClientSocket,
					Buffer = new byte[MaxBufferSize],
					BufferSize = MaxBufferSize,
					StreamBuffer = new MemoryStream()
				};
				var ep = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
				ClientSocket.BeginReceiveFrom(state.Buffer, 0, MaxBufferSize, 0, ref ep, OnReceiveFromCallback, state);
			}
			catch (ObjectDisposedException)
			{
				_listening = false;
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Receive);
				_listening = false;
			}
			catch (Exception e)
			{
				OnCaughtException(e, EventCode.Other);
				_listening = false;
			}
		}

		private void OnReceiveFromCallback(IAsyncResult ar)
		{
			try
			{
				if (ar.AsyncState is ControlState state)
				{
					var ep = new IPEndPoint(IPAddress.Any, 0) as EndPoint;

					var bytesRead = state.CurrentSocket.EndReceiveFrom(ar, ref ep);
					if (bytesRead > 0)
					{
						state.StreamBuffer.Write(state.Buffer, 0, bytesRead);
						state.Buffer = new byte[MaxBufferSize];

						if (state.Buffer.Any(@byte => @byte == '\0'))
						{
							ProcessMessage(state.StreamBuffer);
							state.StreamBuffer = new MemoryStream();
						}
					}
					else if (state.StreamBuffer.CanWrite && state.StreamBuffer.Length > 0)
					{
						ProcessMessage(state.StreamBuffer);
						state.StreamBuffer = new MemoryStream();
					}

					Receive();
				}
			}
			catch (ObjectDisposedException)
			{
				_listening = false;
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Receive);
				_listening = false;
			}
			catch (Exception e)
			{
				OnCaughtException(e, EventCode.Other);
				_listening = false;
			}
		}

		public override void StopService() => (ClientSocket == null || ClientSocket.IsDisposed()
			? (Action) (() => { })
			: ClientSocket.Close)();

		public override void StartService() => Send("");
	}
}