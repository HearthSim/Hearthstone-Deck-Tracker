using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Discover a spell and reduce its Cost by (8). Repeat this with excess Cost reduction."
// EventCount stays 1: the repeats depend on the picked spells' costs.
public class Reload : ClassOrNeutralSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.Reload;
}
