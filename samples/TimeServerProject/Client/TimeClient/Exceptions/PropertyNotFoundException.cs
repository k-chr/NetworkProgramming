using System;

namespace TimeClient.Exceptions
{
	public class PropertyNotFoundException : Exception
	{
		public PropertyNotFoundException(string s) : base(s)
		{
		}
	}
}