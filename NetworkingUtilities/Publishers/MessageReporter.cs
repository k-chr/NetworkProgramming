using System;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Publishers
{
	public class MessageReporter : IReporter<MessageEvent>
	{
		public event EventHandler<MessageEvent> Report;
	}
}