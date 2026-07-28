using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: If you control another Murloc, Discover a Murloc."
public class PrimalfinLookout : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.PrimalfinLookout;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsMurloc()
				&& (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
	}
}
