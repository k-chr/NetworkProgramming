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
			if (obj is ValueTuple<Exception, EventCode> exceptionData)
				Report?.Invoke(this, new ExceptionEvent(exceptionData.Item1, exceptionData.Item2));
		}

		public void AddSubscriber(Action<object, object> procedure) => Report += (s, o) => procedure?.Invoke(s, o);
	}
}