using System;

namespace NetworkingUtilities.Abstracts
{
	public abstract class AbstractClient : ISender, IReceiver, IService
	{
		private readonly IReporter _lastException;
		private readonly IReporter _lastMessage;

		protected AbstractClient(IReporter lastException, IReporter lastMessage)
		{
			_lastException = lastException;
			_lastMessage = lastMessage;
		}

		protected void AddExceptionSubscription(Action<object, object> procedure)
		{
			_lastException.AddSubscriber(procedure);
		}

		protected void AddMessageSubscription(Action<object, object> procedure)
		{
			_lastMessage.AddSubscriber(procedure);
		}

		protected void OnNewMessage(Tuple<string, string, string> messageWithAddresses)
		{
			_lastMessage.Notify(messageWithAddresses);
		}

		protected void OnCaughtException(Exception exception)
		{
			_lastException.Notify(exception);
		}

		public abstract void Send(string message);

		public abstract void Receive();

		public abstract void StopService();

		public abstract void StartService();
	}
}