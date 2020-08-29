using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Extensions;
using NetworkingUtilities.Utilities.Events;
using NetworkingUtilities.Utilities.StateObjects;

namespace NetworkingUtilities.Udp.Multicast
{
	public class MulticastClient : AbstractClient
	{
		private IPAddress _multicastAddress;
		private int _multicastPort;
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

		public override void Send(byte[] data, string to = "")
		{
			try
			{
				var endpoint = new IPEndPoint(_multicastAddress, _multicastPort);
				if (!string.IsNullOrEmpty(to))
				{
					var ep = IPEndPoint.Parse(to);
					if ((!ep.Address.Equals(_multicastAddress) || ep.Port != _multicastPort) &&
						ep.Address.ToString().IsMulticastAddress())
					{
						var old = endpoint;
						endpoint = ep;
						_multicastAddress = endpoint.Address;
						_multicastPort = endpoint.Port;

						OnReportingStatus(StatusCode.Info,
							$"Changed multicast address and port from {old} to {endpoint}");
					}
				}

				ClientSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, endpoint, OnSendToCallback,
					ClientSocket);
				OnReportingStatus(StatusCode.Info,
					$"Started sending {data.Length} bytes via UDP socket in multicast mode");
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
				OnReportingStatus(StatusCode.Success, $"Successfully sent {_} bytes multicast via UDP socket");
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
				OnReportingStatus(StatusCode.Info, "Started receiving bytes via UDP socket");
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
					OnReportingStatus(StatusCode.Success, $"Successfully received {bytesRead} bytes via UDP socket");
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

				if (ClientSocket.LocalEndPoint is IPEndPoint endPoint)
				{
					WhoAmI = new ClientEvent(WhoAmI.Id, endPoint, new IPEndPoint(_multicastAddress, _multicastPort));
				}

				OnReportingStatus(StatusCode.Success, $"Successfully bound to {WhoAmI.Ip}:{WhoAmI.ServerIp}");
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