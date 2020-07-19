using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Extensions;
using NetworkingUtilities.Utilities.Events;
using NetworkingUtilities.Utilities.StateObjects;
using static NetworkingUtilities.Utilities.BroadcastTools;

namespace NetworkingUtilities.Udp.Multicast
{
	public class BroadcastClient : AbstractClient
	{
		private readonly int _port;
		private readonly IPAddress _ipAddress;
		private readonly int _localPort;
		private IPAddress _address;

		public BroadcastClient(string interfaceIp, int port, bool serverHandler = false,
			string ipAddress = null, int localPort = 0) : base(
			new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp), serverHandler)
		{
			_port = port;
			_ipAddress = string.IsNullOrEmpty(ipAddress) ? IPAddress.Any : IPAddress.Parse(ipAddress);
			_localPort = localPort;
			SetBroadcastIp(interfaceIp);
		}

		public override void Send(string message, string to = "")
		{
			throw new NotImplementedException();
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
				ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
				ClientSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 32);
				ClientSocket.Bind(new IPEndPoint(_ipAddress, _localPort));

				if (ClientSocket.LocalEndPoint is IPEndPoint endPoint)
				{
					WhoAmI = new ClientEvent(endPoint.Address, endPoint.Port, WhoAmI.Id);
				}

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

		private void SetBroadcastIp(string selectedIp) => _address = GetBroadcastIpForAddress(selectedIp);
	}
}