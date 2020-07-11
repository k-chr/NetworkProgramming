using System.Net.Sockets;
using System.Threading;

namespace NetworkingUtilities.Utilities.StateObjects
{
	public class WaitState
	{
		public Socket ClientSocket { get; set; }
		public ManualResetEvent BlockingEvent { get; set; }
	}
}