using System;
using System.Net;

namespace NetworkingUtilities.Utilities.Events
{
	public class ClientEvent : EventArgs
	{
		public IPAddress Ip { get; }
		public int Port { get; }
		public string Id { get; }

		public ClientEvent(IPAddress ipAddress, int port, string id = "")
		{
			Ip = ipAddress;
			Port = port;

			if (string.IsNullOrEmpty(id))
			{
				id = $"Client_{new Guid()}";
			}

			Id = id;
		}
	}
}