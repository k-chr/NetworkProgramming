using System;
using System.Collections.Generic;
using System.Text;
using NetworkingUtilities.Abstracts;

namespace NetworkingUtilities.Tcp
{
   public class Client : AbstractClient
   {
	   public Client(IReporter lastException, IReporter lastMessage, IReporter disconnected) : base(lastException, lastMessage, disconnected)
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
