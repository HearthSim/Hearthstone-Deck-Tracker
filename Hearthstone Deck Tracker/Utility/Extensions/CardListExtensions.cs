using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
	public static class CardListExtensions
	{
		public static List<Card> ToSortedCardList(this IEnumerable<Card> cards)
			=> cards.OrderBy(x => x.Cost).ThenBy(x => x.LocalizedName).ToArray().ToList();
	}
}