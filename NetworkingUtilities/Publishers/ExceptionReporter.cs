using System;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Publishers
{
	public class ExceptionReporter : IReporter<ExceptionEvent>, IReporter
	{
		public event EventHandler<ExceptionEvent> Report;

		public void Notify(object obj)
		{
			if (obj is Exception exception)
			{
				Report?.Invoke(this, new ExceptionEvent(exception));
			}
		}

		public void AddSubscriber(Action<object, object> procedure)
		{
			Report += (s, o) => procedure?.Invoke(s, o);
		}
	}
}