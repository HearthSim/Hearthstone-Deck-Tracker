using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Battlecry: Discover a 5, 3, and 1-Cost minion to stitch to this. Deathrattle: Summon the 5-Cost minion."
public class DrStitchensew : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.DrStitchensew;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: 1 or 3 or 5 } && (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
	}
}
