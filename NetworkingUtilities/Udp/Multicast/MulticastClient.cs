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

namespace NetworkingUtilities.Udp.Multicast
{
	public class MulticastClient : AbstractClient
	{
		private readonly IPAddress _multicastAddress;
		private readonly int _multicastPort;
		private readonly IPAddress _ipAddress;
		private readonly int _localPort;

		public MulticastClient(string multicastAddress, int multicastPort, bool serverHandler = false,
			string ipAddress = null, int localPort = 0) : base(
			new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp), serverHandler)
		{
			_multicastAddress = multicastAddress.IsMulticastAddress()
				? IPAddress.Parse(multicastAddress)
				: throw new ArgumentException("Provided address is not in multicast addresses range!");
			_multicastPort = multicastPort;
			_ipAddress = string.IsNullOrEmpty(ipAddress) ? IPAddress.Any : IPAddress.Parse(ipAddress);
			_localPort = localPort;
		}

		public override void Send(string message, string to = "")
		{
			try
			{
				var data = Encoding.ASCII.GetBytes(message);
				var endpoint = new IPEndPoint(_multicastAddress, _multicastPort);

				ClientSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, endpoint, OnSendToCallback,
					ClientSocket);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Send);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception, EventCode.Other);
			}
		}

		private void OnSendToCallback(IAsyncResult ar)
		{
			if (!(ar.AsyncState is Socket socket)) return;

			try
			{
				var _ = socket.EndSendTo(ar);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Send);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception, EventCode.Other);
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
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Receive);
			}
			catch (Exception e)
			{
				OnCaughtException(e, EventCode.Other);
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
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Receive);
			}
			catch (Exception e)
			{
				OnCaughtException(e, EventCode.Other);
			}
		}

		public override void StopService() => (ClientSocket == null || ClientSocket.IsDisposed()
			? (Action) (() => { })
			: ClientSocket.Close)();

		public override void StartService()
		{
			try
			{
				ClientSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 32);
				ClientSocket.Bind(new IPEndPoint(_ipAddress, _localPort));
				Receive();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Bind);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception, EventCode.Other);
			}
		}
	}
}