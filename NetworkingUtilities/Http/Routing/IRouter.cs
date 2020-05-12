namespace NetworkingUtilities.Http.Routing
{
	public interface IRouter
	{
		string Route(string[] segments, string requestHttpMethod);
		void BuildEndPoints();
	}
}