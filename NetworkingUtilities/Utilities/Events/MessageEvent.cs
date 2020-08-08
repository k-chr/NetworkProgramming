using System;

namespace NetworkingUtilities.Utilities.Events
{
	public class MessageEvent : EventArgs
	{
		public byte[] Message { get; }
		public string From { get; }
		public string To { get; }

		public MessageEvent(byte[] message, string from, string to)
		{
			Message = message;
			From = from;
			To = to;
		}
	}
}