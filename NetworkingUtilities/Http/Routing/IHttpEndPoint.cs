namespace NetworkingUtilities.Http.Routing
{
	public interface IHttpEndPoint
	{
		string Invoke(string[] @params);
		bool Matches(string[] segments, string httpMethod);
	}
}
