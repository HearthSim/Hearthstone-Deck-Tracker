using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
	public static class EnumerableExtensions
	{
		public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) => source.Where(x => x != null).Cast<T>();
	}
}
