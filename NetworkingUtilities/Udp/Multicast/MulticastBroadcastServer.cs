using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
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

		public override void Send(string message, string to = "")
		{
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
		}
	}
}