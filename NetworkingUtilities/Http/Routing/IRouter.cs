using System.Net;

namespace NetworkingUtilities.Http.Routing
{
	public interface IRouter
	{
		string Route(string[] segments);
		void BuildEndPoints();

	}
}