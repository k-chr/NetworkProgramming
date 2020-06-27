using System;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Publishers
{
	public class ClientReporter : IReporter, IReporter<ClientEvent>
	{
		public void Notify(object obj)
		{
			if (obj is ClientEvent client)
				Report?.Invoke(this, client);
		}

		public void AddSubscriber(Action<object, object> procedure)
		{
			Report += (s, e) => procedure?.Invoke(s, e);
		}

		public event EventHandler<ClientEvent> Report;
	}
}