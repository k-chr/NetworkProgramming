using System;
using System.IO;
using System.Net;
using NetworkingUtilities.Extensions;
using TimeProjectServices.Enums;

namespace TimeProjectServices.Protocols
{
	public class DiscoverProtocol : IProtocol
	{
		public HeaderType Header { get; } = HeaderType.Discover;
		public ActionType Action { get; set; }

		public IPEndPoint Data { get; set; }

		public byte[] GetBytes()
		{
			var stream = new MemoryStream();
			stream.Write(BitConverter.GetBytes((int) Header));
			stream.Write(BitConverter.GetBytes((int) Action));

			if (Data != null)
				stream.Write(Data.GetBytes());
			return stream.ToArray();
		}
	}
}