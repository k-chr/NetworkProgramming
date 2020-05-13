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

		public string Invoke(string[] @params)
		{
			try
			{
				@params = @params?.Select(param => param.Replace("/", "")).Skip(1).ToArray();
				var args = new List<object>();

				var patternSegments = _pattern.RouteElems.ToList();

				var methodParams = _targetMethod.GetParameters();
				var names = methodParams.Select((info, i) => info.Name).ToList();

				var dict = new Dictionary<string, string>();

				for (var i = 0; i < (@params?.Length ?? 0) && @params != null; ++i)
				{
					var name = patternSegments.FirstOrDefault(element => element.Id == i)?.Key;
					var value = @params[i];
					dict.Add(name, value);
				}

				dict = dict.Where(pair => names.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);



				var obj = _targetMethod.Invoke(_instanceOfController, args.ToArray());
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

			if (segments.Length < patternSegments.Count)
			{
				foreach (var routeElement in patternSegments.Skip(segments.Length))
				{
					if (routeElement is RouteLiteral l || (routeElement is RouteParam p && !p.Optional))
					{
						rV = false;
						break;
					}
				}
			}

			for (var i = 0; i < segments.Length && rV; ++i)
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