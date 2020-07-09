using System;
using System.Linq;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Tcp
{
   public class MultithreadingServer : AbstractServer
   {
	   public MultithreadingServer(string ip, int port, string interfaceName, int maxClientsQueue = 3) : base(ip, port, interfaceName)
	   {
	   }

	  private void RegisterHandler(AbstractClient handler)
	  {
		  handler.AddExceptionSubscription((o, o1) =>
		  {
			  if (o1 is ExceptionEvent e)
				  OnCaughtException(e.LastError, e.LastErrorCode);
		  });

		  handler.AddMessageSubscription((o, o1) =>
		  {
			  if (o1 is MessageEvent @event)
				  OnNewMessage(@event.Message, @event.From, @event.To);
		  });

		  handler.AddOnDisconnectedSubscription((o, o1) =>
		  {
			  if (o1 is ClientEvent @event)
			  {
				  OnDisconnect(@event.Ip, @event.Id, @event.Port);
				  Clients.Remove(Clients.FirstOrDefault());
			  }
		  });
		  handler.StartService();
	  }

	  public void StopService()
	  {
		  DisposeCurrentSession();
	  }

	  private void DisposeCurrentSession()
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
