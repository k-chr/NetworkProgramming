using System;

namespace NetworkingUtilities.Abstracts
{
	public interface IReporter<T>
	{
		event EventHandler<T> Report;
	}

	public interface IReporter
	{
		void Notify(object obj);
		void AddSubscriber(Action<object, object> procedure);
	}
}