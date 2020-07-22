using System.Net.Sockets;

namespace NetworkingUtilities.Utilities.StateObjects
{
	public class ReceiverState
	{
		public Socket Socket;
		public string Ip;
		public int Port;
	}
}
