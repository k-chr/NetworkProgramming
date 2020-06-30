using System;
using System.Collections.Generic;

namespace NetworkingUtilities.Http.Routing
{
	public class RouteParam : IRouteElement
	{
		public RouteParam(string key, int id, bool optional, Dictionary<string, Func<object, bool>> constraints,
			IEnumerable<string> defaults)
		{
			Key = key;
			Id = id;
			Optional = optional;
			Constraints = constraints;
			Defaults = defaults;
		}

		public string Key { get; }
		public int Id { get; }
		public bool Optional { get; }
		public Dictionary<string, Func<object, bool>> Constraints { get; }
		public IEnumerable<string> Defaults { get; }
	}
}