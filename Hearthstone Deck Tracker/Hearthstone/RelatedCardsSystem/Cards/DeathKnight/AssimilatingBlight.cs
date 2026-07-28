using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Discover a 3-Cost Deathrattle minion. Summon it with Reborn."
public class AssimilatingBlight : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.AssimilatingBlight;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: 3 }
				&& (c.IsClass(playerClass) || c.IsClass("Neutral")) && c.HasDeathrattle())
			.Select(c => new Card(c));
	}
}
