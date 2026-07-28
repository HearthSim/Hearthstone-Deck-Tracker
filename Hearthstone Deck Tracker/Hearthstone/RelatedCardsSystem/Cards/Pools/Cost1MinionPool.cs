using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

// Shared pool. Cards inherit this for the card pool only; each card declares its own
// Picks()/EventCount()/IsWithReplacement().
public abstract class Cost1MinionPool : DiscoverPoolCard
{
	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: 1 })
			.Select(c => new Card(c));
	}
}
