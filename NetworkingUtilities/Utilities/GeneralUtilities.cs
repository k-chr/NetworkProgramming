using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace NetworkingUtilities.Utilities
{
	public static class GeneralUtilities
	{
		public static List<NetworkInterface> GetNetworkInterfacesThatAreUp() =>
			NetworkInterface.GetAllNetworkInterfaces().Where(networkInterface =>
				networkInterface.OperationalStatus == OperationalStatus.Up).ToList();
	}
}