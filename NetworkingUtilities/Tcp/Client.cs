using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using NetworkingUtilities.Abstracts;

namespace NetworkingUtilities.Tcp
{
   public class Client : AbstractClient
   {
	   public Client(Socket socket, IReporter lastException, IReporter lastMessage, IReporter disconnected) : base(socket, lastException, lastMessage, disconnected)
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
