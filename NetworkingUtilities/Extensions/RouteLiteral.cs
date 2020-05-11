namespace NetworkingUtilities.Extensions
{
	public class RouteLiteral
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
