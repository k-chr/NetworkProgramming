using System;

namespace NetworkingUtilities.Extensions
{
	public static class BasicTypeExtensions
	{
		public static bool InRange(this int testedValue, int min, int max) => testedValue >= Math.Min(min, max) && testedValue <= Math.Max(min, max);
	}
}
