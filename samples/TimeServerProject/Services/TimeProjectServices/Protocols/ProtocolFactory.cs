using System;
using System.Net;
using TimeProjectServices.Enums;

namespace TimeProjectServices.Protocols
{
	public static class ProtocolFactory
	{
		public static IProtocol FromBytes(byte[] bytes)
		{
			try
			{
				IProtocol protocol;
				var headerValue = BitConverter.ToInt32(bytes[..4]);
				var action = (ActionType) BitConverter.ToInt32(bytes[4..8]);
				var dataBytes = bytes[8..];
				switch ((HeaderType) headerValue)
				{
					case HeaderType.Discover:
						var data = action == ActionType.Response
							? new IPEndPoint(new IPAddress(dataBytes[..4]), BitConverter.ToInt32(dataBytes[4..]))
							: null;
						protocol = new DiscoverProtocol
						{
							Data = data
						};
						break;
					case HeaderType.Time:
						var timeData = action == ActionType.Response
							? DateTimeOffset.FromUnixTimeMilliseconds(BitConverter.ToInt64(dataBytes))
							: default;
						protocol = new TimeProtocol
						{
							Data = timeData
						};
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				protocol.Action = action;

				return protocol;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}