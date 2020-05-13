using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetworkingUtilities.Http.Attributes;

namespace NetworkingUtilities.Http.Routing
{
	public class Router : IRouter
	{
		public string Route(string[] segments, string requestHttpMethod)
		{
			var endPoint = _endPoints.FirstOrDefault(e => e.Matches(segments, requestHttpMethod));

			return endPoint?.Invoke(segments);
		}

		public void BuildEndPoints()
		{
			var endPoints = new List<IHttpEndPoint>();

			try
			{
				var collection = Assembly.GetEntryAssembly()?.GetTypes();
				var collection2 = collection?.Where(t => t.GetInterfaces().Contains(typeof(IController))).ToList();
				foreach (var type in collection2??new List<Type>())
				{
					var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance );
					methods = methods.Where(m =>
					m.GetCustomAttributes(true).Any(attribute => attribute is ControllerRouteAttribute)).ToArray();

					foreach (var methodInfo in methods)
					{
						if (methodInfo.GetCustomAttributes(true)
							.FirstOrDefault(attribute => attribute is ControllerRouteAttribute) is ControllerRouteAttribute attr)
						{
							var verb = attr.Verb;
							var template = attr.Template;
							var instance = Activator.CreateInstance(type, null);
							var pattern = new RouteParser().ParsePattern(template);
							var endPoint = new HttpEndPoint(methodInfo, (IController)instance, pattern, new List<string> { verb });
							endPoints.Add(endPoint);
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			_endPoints = endPoints;
		}

		private ICollection<IHttpEndPoint> _endPoints;
	}
}
