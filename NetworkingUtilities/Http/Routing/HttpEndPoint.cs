using System;
using System.Reflection;

namespace NetworkingUtilities.Http.Routing
{
	public class HttpEndPoint : IHttpEndPoint
	{
		private readonly MethodInfo _targetMethod;
		private readonly IController _instanceOfController;

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

			return false;
		}

		public HttpEndPoint(MethodInfo targetMethod, IController instanceOfController)
		{
			_targetMethod = targetMethod;
			_instanceOfController = instanceOfController;
		}
	}
}