using System;

namespace NetworkingUtilities.Utilities.Events
{
	public class ExceptionEvent : EventArgs
	{
		public Exception LastError { get; }

		public ExceptionEvent(Exception happenedException)
		{
			LastError = happenedException;
		}
	}
}