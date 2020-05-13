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

				foreach (var (key, value) in dict)
				{
					var segment = patternSegments.FirstOrDefault(element => element.Key.Equals(key));
					if (segment != null)
					{
						if (segment is RouteParam param)
						{
							var constraints = param.Constraints;
							if (constraints.ContainsKey("intRange"))
							{
								var valueObject = value.Split('_').Select(int.Parse).ToArray();
								args.Add(valueObject);
							}

							else if (constraints.ContainsKey("int"))
							{
								var valueObject = int.Parse(value);

								Func<object, bool> func;
								var test = true;
								
								if (constraints.ContainsKey("min"))
								{
									func = constraints["min"];
									test = func(valueObject);
								}

								if (!test) return null;

								if (constraints.ContainsKey("max"))
								{
									func = constraints["max"];
									test = func(valueObject);
								}

								if (!test) return null;

								if (constraints.ContainsKey("inRange"))
								{
									func = constraints["inRange"];
									test = func(valueObject);
								}

								if (!test) return null;
								args.Add(valueObject);
							}

							else if (constraints.ContainsKey("alpha"))
							{
								var test = value.All(char.IsLetter);

								if (constraints.ContainsKey("length") && test)
								{
									var func = constraints["inRange"];
									test = func(value);
								}

								if (!test) return null;
								args.Add(value);
							}

							else if (constraints.ContainsKey("length"))
							{
								var func = constraints["inRange"];
								var test = func(value);
								if (!test) return null;
								args.Add(value);
							}

							else if (constraints.ContainsKey("date"))
							{
								var test = DateTime.TryParse(value, out var val);
								if (test)
								{
									args.Add(val);
								}
							}

							else
							{
								args.Add(value);
							}
						}
					}
				}

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
					if (routeElement is RouteLiteral || (routeElement is RouteParam p && !p.Optional))
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