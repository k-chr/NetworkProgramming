namespace NetworkingUtilities.Http.Routing
{
	public class RouteLiteral : IRouteElement
	{
		public int Id { get; }
		public string Key { get; }

		public RouteLiteral(string key, int id)
		{
			Key = key;
			Id = id;
		}
	}
}