using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NetworkingUtilities.Http.Attributes;

namespace NetworkingUtilities.Http.Routing
{
	public class Router : IRouter
	{
		public string Route(string[] segments, string requestHttpMethod)
		{


			return null;
		}

		public void BuildEndPoints()
		{
			var endPoints = new List<IHttpEndPoint>();

			try
			{
				foreach (var type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IController))))
				{
					var methods = type.GetMethods().Where(m =>
						m.GetCustomAttributes(true).Any(attribute => attribute is ControllerRouteAttribute));

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
