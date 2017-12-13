using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
	public static class CardListExtensions
	{
		public static List<Card> ToSortedCardList(this IEnumerable<Card> cards)
			=> cards.OrderByDescending(x => x.HideStats).ThenBy(x => x.Cost).ThenBy(x => x.LocalizedName).ToArray().ToList();
	}
}
