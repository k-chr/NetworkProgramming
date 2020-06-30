using System;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Publishers
{
	public class MessageReporter : IReporter<MessageEvent>, IReporter
	{
		public event EventHandler<MessageEvent> Report;

		public void Notify(object obj)
		{
			if (obj is Tuple<string, string, string> message)
			{
				Report?.Invoke(this, new MessageEvent(message.Item1, message.Item2, message.Item3));
			}
		}

		public void AddSubscriber(Action<object, object> procedure)
		{
			Report += (s, o) => procedure?.Invoke(s, o);
		}
	}
}