using System;

namespace TimeProjectServices.Exceptions
{
	public class PropertyNotFoundException : Exception
	{
		public PropertyNotFoundException(string s) : base(s)
		{
		}
	}
}