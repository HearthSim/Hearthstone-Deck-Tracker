using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
	public static class CardListExtensions
	{
		public static List<Card> ToSortedCardList(this IEnumerable<Card> cards)
			=> cards.OrderByDescending(x => x.HideStats).ThenBy(x => x.Cost).ThenBy(x => x.LocalizedName).ToArray().ToList();

		public static List<Card> ToDiffCardList(this IEnumerable<Card> cards, List<Card> newCards)
		{
			var diff = new List<Card>();

			//removed
			foreach(var c in cards.Where(c => !newCards.Contains(c)))
			{
				var cd = c.Clone() as Card;
				if(cd == null)
					continue;
				cd.Count = -cd.Count; //mark as negative for visual
				diff.Add(cd);
			}

			//added
			diff.AddRange(newCards.Where(c => !cards.Contains(c)));

			//diff count
			var diffCount =
				newCards.Where(c => cards.Any(c2 => c2.Id == c.Id) && cards.First(c2 => c2.Id == c.Id).Count != c.Count);
			foreach(var card in diffCount)
			{
				var cardclone = card.Clone() as Card;
				if(cardclone == null)
					continue;
				cardclone.Count -= cards.First(c => c.Id == cardclone.Id).Count;
				diff.Add(cardclone);
			}

			return diff;
		}
	}
}
