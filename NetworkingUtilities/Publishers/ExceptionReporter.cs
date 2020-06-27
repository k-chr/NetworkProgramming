using System;
using NetworkingUtilities.Abstracts;
using NetworkingUtilities.Utilities.Events;

namespace NetworkingUtilities.Publishers
{
	public class ExceptionReporter : IReporter<ExceptionEvent>
	{
		public event EventHandler<ExceptionEvent> Report;
	}
}