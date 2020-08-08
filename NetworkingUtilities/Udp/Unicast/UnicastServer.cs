using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Extensions;
using NetworkingUtilities.Utilities.Events;
using NetworkingUtilities.Utilities.StateObjects;

namespace NetworkingUtilities.Udp.Unicast
{
	public class UnicastServer : AbstractServer, IReceiver
	{
		private readonly Dictionary<EndPoint, ControlState> _clientsBuffers;

		public UnicastServer(string ip, int port, string interfaceName) : base(ip, port, interfaceName)
		{
			_clientsBuffers = new Dictionary<EndPoint, ControlState>();
			ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		}

		public override void Send(byte[] data, string to = "")
		{
			try
			{
				var endpoint = IPEndPoint.Parse(to);
				var state = new ReceiverState
				{
					Port = endpoint.Port,
					Socket = ServerSocket,
					Ip = endpoint.Address.ToString()
				};

				ServerSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, endpoint, OnSendToCallback, state);
				OnReportingStatus(StatusCode.Info, $"Started sending {data.Length} bytes to {endpoint} via UDP socket");
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
				if (!(ar.AsyncState is ReceiverState state)) return;
				var _ = state.Socket.EndSendTo(ar);
				OnReportingStatus(StatusCode.Success, $"Successfully send {_} bytes to {state.Ip}:{state.Port} via UDP socket");
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

		public override void StopService() => (ServerSocket == null || ServerSocket.IsDisposed()
			? (Action) (() => { })
			: ServerSocket.Close)();

		public override void StartService()
		{
			try
			{
				ServerSocket.Bind(new IPEndPoint(IPAddress.Parse(Ip), Port));
				OnReportingStatus(StatusCode.Success, $"Successfully bound to {ServerSocket.LocalEndPoint}");
				Receive();
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Bind);
			}
			catch (Exception e)
			{
				OnCaughtException(e, EventCode.Other);
			}
		}

		public void Receive()
		{
			try
			{
				var state = new ControlState()
				{
					CurrentSocket = ServerSocket,
					BufferSize = MaxBufferSize,
					Buffer = new byte[MaxBufferSize],
					StreamBuffer = new MemoryStream()
				};

				var endpoint = new IPEndPoint(IPAddress.Any, 0) as EndPoint;

				ServerSocket.BeginReceiveFrom(state.Buffer, 0, MaxBufferSize, SocketFlags.None, ref endpoint,
					OnReceiveFromCallback, state);
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
				var end = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
				if (!(ar.AsyncState is ControlState state)) return;
				var bytesRead = state.CurrentSocket.EndReceiveFrom(ar, ref end);
				OnReportingStatus(StatusCode.Success, $"Successfully received {bytesRead} bytes from {end} via UDP socket");
				if (!_clientsBuffers.ContainsKey(end))
				{
					var s = new ControlState
					{
						Buffer = new byte[MaxBufferSize],
						BufferSize = MaxBufferSize,
						StreamBuffer = new MemoryStream(),
					};
					_clientsBuffers.Add(end, s);
				}

				if (bytesRead > 0)
				{
					_clientsBuffers[end].StreamBuffer.Write(state.Buffer, 0, bytesRead);
					if (state.Buffer.Any(@byte => @byte == '\0'))
					{
						ProcessMessage(end);
						_clientsBuffers[end].StreamBuffer = new MemoryStream();
					}
				}
				else if (_clientsBuffers[end].StreamBuffer.CanWrite && _clientsBuffers[end].StreamBuffer.Length > 0)
				{
					ProcessMessage(end);
					_clientsBuffers[end].StreamBuffer = new MemoryStream();
				}

				var e = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
				state.CurrentSocket.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ref e,
					OnReceiveFromCallback, state);
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

		private void ProcessMessage(EndPoint end)
		{
			if (!_clientsBuffers.ContainsKey(end)) return;
			var state = _clientsBuffers[end];
			using var stream = state.StreamBuffer;
			stream.Seek(0, SeekOrigin.Begin);
			OnNewMessage(stream.ToArray(), ((IPEndPoint) end).ToString(), "server");
		}
	}
}