using System;
using System.Collections.Generic;
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
	class MulticastBroadcastServer : AbstractServer, IReceiver
	{
		private readonly bool _acceptBroadcast;
		private readonly Dictionary<EndPoint, ControlState> _clientsBuffers;

		public MulticastBroadcastServer(string ip, int port, string interfaceName, bool acceptBroadcast = false) : base(
			ip, port, interfaceName)
		{
			_acceptBroadcast = acceptBroadcast;
			ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			_clientsBuffers = new Dictionary<EndPoint, ControlState>();
		}

		public override void StopService() =>
			(ServerSocket == null || ServerSocket.IsDisposed()
				? (Action) (() => { })
				: () =>
				{
					if (!_acceptBroadcast)
						ServerSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership,
							new MulticastOption(IPAddress.Parse(Ip)));
					ServerSocket.Close();
				})();


		public override void StartService()
		{
			InitializeSocket();
			Receive();
		}

		private void InitializeSocket()
		{
			try
			{
				var localAdd = IPAddress.Any;
				var groupAddress = IPAddress.Parse(Ip);

				if (!_acceptBroadcast)
				{
					ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, false);
					ServerSocket.EnableBroadcast = false;
					ServerSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
						new MulticastOption(groupAddress, localAdd));
				}
				else
				{
					ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
					ServerSocket.EnableBroadcast = true;
					ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				}

				ServerSocket.Bind(new IPEndPoint(localAdd, Port));
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
				var state = new ControlState
				{
					CurrentSocket = ServerSocket,
					BufferSize = MaxBufferSize,
					Buffer = new byte[MaxBufferSize],
					StreamBuffer = new MemoryStream()
				};
				var endPoint = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
				ServerSocket.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, 0, ref endPoint,
					OnReceiveFromCallback, state);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Receive);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception, EventCode.Other);
			}
		}

		private void OnReceiveFromCallback(IAsyncResult ar)
		{
			try
			{
				var end = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
				if (!(ar.AsyncState is ControlState state)) return;
				var bytesRead = state.CurrentSocket.EndReceiveFrom(ar, ref end);

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
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException socketException)
			{
				OnCaughtException(socketException, EventCode.Receive);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception, EventCode.Other);
			}
		}

		private void ProcessMessage(EndPoint end)
		{
			if (!_clientsBuffers.ContainsKey(end)) return;
			var state = _clientsBuffers[end];
			using var stream = state.StreamBuffer;
			stream.Seek(0, SeekOrigin.Begin);
			var message = Encoding.UTF8.GetString(stream.ToArray());
			OnNewMessage(message, ((IPEndPoint) end).ToString(), "server");
		}

		public override void Send(string message, string to = "")
		{
			try
			{
				var data = Encoding.ASCII.GetBytes(message);
				var endpoint = IPEndPoint.Parse(to);
				var state = new ReceiverState
				{
					Port = endpoint.Port,
					Socket = ServerSocket,
					Ip = endpoint.Address.ToString()
				};

				ServerSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, endpoint, OnSendToCallback, state);
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
				OnNewMessage($"Data were successfully sent to {state.Ip}:{state.Port}", "server", "server");
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
	}
}