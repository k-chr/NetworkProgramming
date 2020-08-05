using System.Collections.Generic;

namespace NetworkingUtilities.Extensions
{
	public static class CollectionsExtensions
	{
		public static T Get<TK, T>(this Dictionary<TK, T> dictionary, TK key) =>
			dictionary.ContainsKey(key) ? dictionary[key] : default;
	}
}