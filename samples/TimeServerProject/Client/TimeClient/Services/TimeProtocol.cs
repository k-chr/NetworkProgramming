using System;
using System.IO;
using NetworkingUtilities.Extensions;

namespace TimeClient.Services
{
	class TimeProtocol : IProtocol
	{
		public HeaderType Header { get; } = HeaderType.Time;
		public ActionType Action { get; set; }

		public DateTimeOffset Data { get; set; }

		public byte[] GetBytes()
		{
			var stream = new MemoryStream();
			stream.Write(BitConverter.GetBytes((int)Header));
			stream.Write(BitConverter.GetBytes((int)Action));
			if (Data != default)
				stream.Write(Data.ToUnixTimeMilliseconds().GetBytes());
			return stream.ToArray();
		}
	}
}
