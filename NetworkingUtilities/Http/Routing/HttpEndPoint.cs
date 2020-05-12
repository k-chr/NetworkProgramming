using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetworkingUtilities.Http.Routing
{
	public class HttpEndPoint : IHttpEndPoint
	{
		private readonly MethodInfo _targetMethod;
		private readonly IController _instanceOfController;
		private readonly RoutePattern _pattern;
		private readonly List<string> _supportedMethods;
		public string Invoke(object[] @params)
		{
			try
			{
				var obj = _targetMethod?.Invoke(_instanceOfController, @params);
				if (obj is string s) return s;
			}
			catch (Exception e)
			{
				Console.Write(e);
			}


			return null;
		}

		public bool Matches(string[] segments, string httpMethod)
		{
			if (!_supportedMethods.Contains(httpMethod)) return false;

			var patternSegments = _pattern.RouteElems;

			segments = segments.Skip(1).ToArray();

			var rV = true;

			for(var i = 0; i < segments.Length && rV; ++i)
			{
				var segment = segments[i].Replace("/", "");
				var elem = patternSegments.FirstOrDefault(seg => seg.Id == i);
				if (elem == null)
				{
					rV = false;
					break;
				}

				switch (elem)
				{
					case RouteParam p:
						var matchers = p.Constraints;
						rV = matchers.All(kvp => kvp.Value(segment));
						break;
					case RouteLiteral l:
						rV = l.Key.Equals(segment);
						break;
				}
			}



			return rV;
		}

		public HttpEndPoint(MethodInfo targetMethod, IController instanceOfController, RoutePattern pattern, List<string> supportedMethods)
		{
			_targetMethod = targetMethod;
			_instanceOfController = instanceOfController;
			_supportedMethods = supportedMethods;
			_pattern = pattern;
		}
	}
}