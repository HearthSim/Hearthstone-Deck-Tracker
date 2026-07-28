using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Discover an odd-Attack Beast. If you play it this turn, also pick one of the others."
public class OddMap : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.OddMap;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION }
				&& c.IsBeast() && c.Attack % 2 != 0 && (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
	}
}
