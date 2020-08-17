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

		public static IProtocol CreateProtocol(ActionType action, HeaderType header, object data = null)
		{
			IProtocol protocol = header switch
								 {
									 HeaderType.Discover => new DiscoverProtocol {Data = data as IPEndPoint},
									 HeaderType.Time => new TimeProtocol {Data = (DateTimeOffset?) data ?? default},
									 _ => throw new ArgumentOutOfRangeException(nameof(header), header, null)
								 };

			protocol.Action = action;
			return protocol;
		}
	}
}