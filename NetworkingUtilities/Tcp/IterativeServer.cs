using System;
using System.Collections.Generic;
using System.Text;
using NetworkingUtilities.Abstracts;

namespace NetworkingUtilities.Tcp
{
   public class IterativeServer : AbstractServer
   {
	   public IterativeServer(IReporter disconnected, IReporter lastMessage, IReporter lastException, IReporter newClient) : base(disconnected, lastMessage, lastException, newClient)
	   {
	   }

	   public override void Send(string message, string to = "")
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
