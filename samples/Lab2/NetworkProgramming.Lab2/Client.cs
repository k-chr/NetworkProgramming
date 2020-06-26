using System.Threading;

namespace NetworkProgramming.Lab2
{
	public class Client : AbstractClient
	{
		protected override string OnSendErrorMessage =>
			"Cannot send provided message to server due to connection issues!\n";

		protected override string OnConnectErrorMessage => "Failed to connect to remote server\n";
		protected override string OnConnectSuccessMessage => "Succeeded in connecting to remote server\n";
		protected override string OnDisconnectSuccessMessage => "Successfully disconnected\n";
		protected override string OnDisconnectErrorMessage => "Can't properly disconnect from host\n";
		protected override string OnReceiveErrorMessage => "Failed to receive message due to connection issues\n";

		public Client(string address, int port, ManualResetEvent manualResetEvent) : base(address, port,
			manualResetEvent)
		{
		}
	}
}