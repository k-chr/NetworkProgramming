using System;
using System.Net;

namespace NetworkingUtilities.Extensions
{
	public static class BasicTypeExtensions
	{
		public static bool InRange(this int testedValue, int min, int max) =>
			testedValue >= Math.Min(min, max) && testedValue <= Math.Max(min, max);

		public static bool IsMulticastAddress(this string address)
		{
			if (string.IsNullOrEmpty(address)) return false;

			if (!IPAddress.TryParse(address, out var outValue)) return false;
			var @byte = outValue.GetAddressBytes()[0];
			return ((int) @byte).InRange(224, 239);
		}

		public static byte[] GetBytes(this long num) => BitConverter.GetBytes(num);
		public static byte[] GetBytes(this int num) => BitConverter.GetBytes(num);

		public static byte[] GetBytes(this IPEndPoint endPoint) =>
			endPoint.Address.GetAddressBytes().Concat(endPoint.Port.GetBytes());
	}
}