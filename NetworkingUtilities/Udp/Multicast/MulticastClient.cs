using System.Net.Sockets;
using NetworkingUtilities.Abstracts;

namespace NetworkingUtilities.Udp.Multicast
{
   class MulticastClient : AbstractClient
   {
	   public MulticastClient(Socket clientSocket, bool serverHandler = false) : base(clientSocket, serverHandler)
	   {
	   }

	   public override void Send(string message, string to = "")
	   {
		   throw new System.NotImplementedException();
	   }

	   public override void Receive()
	   {
		   throw new System.NotImplementedException();
	   }

	   public override void StopService()
	   {
		   throw new System.NotImplementedException();
	   }

	   public override void StartService()
	   {
		   throw new System.NotImplementedException();
	   }
   }
}
