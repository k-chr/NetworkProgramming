namespace NetworkingUtilities.Utilities.Events
{
	public class StatusEvent
	{
		public StatusCode StatusCode { get; }
		public string StatusMessage { get; }

		public StatusEvent(StatusCode code, string message)
		{
			StatusCode = code;
			StatusMessage = message;
		}
	}
}