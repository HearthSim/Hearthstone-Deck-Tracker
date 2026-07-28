using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

// Shared pool. Cards inherit this for the card pool only; each card declares its own
// Picks()/EventCount()/IsWithReplacement().
public abstract class ClassOrNeutralCost2CardPool : DiscoverPoolCard
{
	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.Cost == 2 && (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
	}
}
