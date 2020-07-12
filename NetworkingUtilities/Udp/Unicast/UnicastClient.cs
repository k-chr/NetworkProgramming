using System;
using System.Net.Sockets;
using NetworkingUtilities.Abstracts;

namespace NetworkingUtilities.Udp.Unicast
{
   class UnicastClient : AbstractClient
   {
	   public UnicastClient(Socket clientSocket, bool serverHandler = false) : base(clientSocket, serverHandler)
	   {
	   }

	   public override void Send(string message, string to = "")
	   {
		   throw new NotImplementedException();
	   }

	   public override void Receive()
	   {
		   throw new NotImplementedException();
	   }

	   public override void StopService()
	   {
		   throw new NotImplementedException();
	   }

	   public override void StartService()
	   {
		   throw new NotImplementedException();
	   }
   }
}
