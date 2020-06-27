using System;
using NetworkingUtilities.Abstracts;

namespace NetworkingUtilities.Tcp	
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

		public void AddExceptionSubscription()
		{

		}

		public void AddMessageSubscription()
		{

		}

		private void OnNewMessage(Tuple<string, string, string> messageWithAddresses)
		{
			_lastMessage.Notify(messageWithAddresses);
		}

		private void OnCaughtException(Exception exception)
		{
			_lastException.Notify(exception);
		}

		public void Send(string message)
		{
		}

		public void Receive()
		{
			throw new NotImplementedException();
		}

		public void StopService()
		{
			throw new NotImplementedException();
		}

		public void StartService()
		{
			throw new NotImplementedException();
		}
	}
}