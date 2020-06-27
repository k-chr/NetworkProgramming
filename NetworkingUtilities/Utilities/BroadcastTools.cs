using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NetworkingUtilities.Utilities
{
	public static class BroadcastTools
	{
		private static IPAddress GetSubnetMask(IPAddress address)
		{
			foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
			{
				foreach (var uniCastIpAddressInformation in adapter.GetIPProperties().UnicastAddresses)
				{
					if (uniCastIpAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork &&
						address.Equals(uniCastIpAddressInformation.Address))
					{
						return uniCastIpAddressInformation.IPv4Mask;
					}
				}
			}

			throw new ArgumentException($"Can't find subnet mask for provided IPv4 address '{address}'");
		}

		private static (IPAddress mask, IPAddress address) ObtainMaskAndLocalIp(string selectedIp)
		{
			var ipAddress = IPAddress.Parse(selectedIp);
			return (GetSubnetMask(ipAddress), ipAddress);
		}

		public static IPAddress GetBroadcastIpForAddress(string selectedIp)
		{
			var (mask, address) = ObtainMaskAndLocalIp(selectedIp);
			var ipAddress = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
			var ipMaskV4 = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);
			var broadCastIpAddress = ipAddress | ~ipMaskV4;
			return new IPAddress(BitConverter.GetBytes(broadCastIpAddress));
		}
	}
}