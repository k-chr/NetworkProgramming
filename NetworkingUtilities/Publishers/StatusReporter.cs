using System;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Publishers
{
	public class StatusReporter : IReporter, IReporter<StatusEvent>
	{
		public event EventHandler<StatusEvent> Report;

		public void Notify(object obj)
		{
			if (obj is ValueTuple<StatusCode, string> data)
				Report?.Invoke(this, new StatusEvent(data.Item1, data.Item2));
		}

		public void AddSubscriber(Action<object, object> procedure) => Report += (s, e) => procedure?.Invoke(s, e);
	}
}
