using System;
using System.Net;

namespace NetworkingUtilities.Utilities.Events
{
	public class ClientEvent : EventArgs
	{
		public IPEndPoint Ip { get; }
		public IPEndPoint ServerIp { get; }
		public string Id { get; }

		public ClientEvent(string id, IPEndPoint clientIp, IPEndPoint serverIp)
		{
			Ip = clientIp;
			ServerIp = serverIp;

			if (string.IsNullOrEmpty(id))
			{
				id = $"Client_{Guid.NewGuid()}";
			}

			Id = id;
		}
	}
}