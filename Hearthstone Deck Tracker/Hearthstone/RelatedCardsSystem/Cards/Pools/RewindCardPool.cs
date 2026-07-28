using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

// Shared pool. Cards inherit this for the card pool only; each card declares its own
// Picks()/EventCount()/IsWithReplacement().
public abstract class RewindCardPool : DiscoverPoolCard
{
	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		// "from any class" — class cards only, the in-game pool has no Neutral Rewinds.
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.HasTag(GameTag.REWIND) && c.Class != CardClass.NEUTRAL)
			.Select(c => new Card(c));
	}
}
