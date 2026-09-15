using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
	public static class EnumerableExtensions
	{
		public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) => source.Where(x => x != null).Cast<T>();

		public static bool IsEmpty<T>(this IEnumerable<T>? source) => source == null || !source.Any();

		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
		{
			foreach(var item in items)
				collection.Add(item);
		}
	}
}
