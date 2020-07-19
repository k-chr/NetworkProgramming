using System;
using System.Net;
using System.Net.Sockets;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Extensions;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Udp.Multicast
{
	public class MulticastClient : AbstractClient
	{
		private readonly string _multicastAddress;
		private readonly int _multicastPort;
		private readonly string _ipAddress;
		private readonly int _localPort;

		public MulticastClient(string multicastAddress, int multicastPort, bool serverHandler = false,
			string ipAddress = null, int localPort = 0) : base(
			new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp), serverHandler)
		{
			_multicastAddress = multicastAddress;
			_multicastPort = multicastPort;
			_ipAddress = ipAddress;
			_localPort = localPort;
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
			catch (Exception exception)
			{
				OnCaughtException(exception, EventCode.Other);
			}
		}

		public override void Receive()
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
				OnCaughtException(socketException, EventCode.Receive);
			}
			catch (Exception exception)
			{
				OnCaughtException(exception, EventCode.Other);
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
				ClientSocket.Bind(new IPEndPoint(
					string.IsNullOrEmpty(_ipAddress) ? IPAddress.Any : IPAddress.Parse(_ipAddress), _localPort));
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