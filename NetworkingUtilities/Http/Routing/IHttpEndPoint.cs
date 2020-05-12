namespace NetworkingUtilities.Http.Routing
{
	public interface IHttpEndPoint
	{
		string Invoke(object[] @params);
		bool Matches(string[] segments, string httpMethod);
	}
}
