using System;
using System.Net;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Publishers
{
	public class ClientReporter : IReporter, IReporter<ClientEvent>
	{
		public void Notify(object obj)
		{
			if (obj is Tuple<IPAddress, string, int> data)
				Report?.Invoke(this, new ClientEvent(data.Item1, data.Item3, data.Item2));
		}

		public void AddSubscriber(Action<object, object> procedure)
		{
			Report += (s, e) => procedure?.Invoke(s, e);
		}

		public event EventHandler<ClientEvent> Report;
	}
}