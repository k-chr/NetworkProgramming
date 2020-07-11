using System;

namespace NetworkingUtilities.Utilities.Events
{
	public class ExceptionEvent : EventArgs
	{
		public Exception LastError { get; }
		public EventCode LastErrorCode { get; }

		public ExceptionEvent(Exception happenedException, EventCode code)
		{
			LastError = happenedException;
			LastErrorCode = code;
		}
	}
}