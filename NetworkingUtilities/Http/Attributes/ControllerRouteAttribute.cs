using System;

namespace NetworkingUtilities.Http.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class ControllerRouteAttribute : Attribute
	{
		public string Template { get; }
		public string Verb { get; }

		public ControllerRouteAttribute(string url, string verb = "GET") : base()
		{
			Template = url;
			Verb = verb;
		}
	}
}