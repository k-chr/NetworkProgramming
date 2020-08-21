using System;
using System.Collections.Generic;

namespace NetworkingUtilities.Extensions
{
	public static class CollectionsExtensions
	{
		public static T Get<TK, T>(this Dictionary<TK, T> dictionary, TK key) =>
			dictionary.ContainsKey(key) ? dictionary[key] : default;

		public static T[] Concat<T>(this T[] source, T[] other)
		{
			var destination = new T[source.Length + other.Length];
			Array.Copy(source, 0, destination, 0, source.Length);
			Array.Copy(other, 0, destination, source.Length, other.Length);
			return destination;
		}
	}
}