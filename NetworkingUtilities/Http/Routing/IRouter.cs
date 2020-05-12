using System.Net;

namespace NetworkingUtilities.Http.Routing
{
	public interface IRouter
	{
		string Route(HttpListenerContext ctx);
		void BuildEndPoints();

	}
}