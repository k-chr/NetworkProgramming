using System;

namespace NetworkingUtilities.Abstracts
{
	public interface IReporter<T>
	{
		event EventHandler<T> Report;
	}
}