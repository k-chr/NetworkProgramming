using System;
using System.Collections.Generic;
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
		}

		public override void Send(string message, string to = "")
		{
			try
			{
				throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public void Receive()
		{
			try
			{

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
	}
}