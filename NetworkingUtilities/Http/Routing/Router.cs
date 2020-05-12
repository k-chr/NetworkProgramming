using System.Collections.Generic;
using System.Net;

namespace NetworkingUtilities.Http.Routing
{
	public class Router : IRouter
	{
		public string Route(string[] segments, string requestHttpMethod)
		{
			return "";
		}

		public void BuildEndPoints()
		{
			throw new System.NotImplementedException();
		}

		private ICollection<IHttpEndPoint> _endPoints;
	}
}
