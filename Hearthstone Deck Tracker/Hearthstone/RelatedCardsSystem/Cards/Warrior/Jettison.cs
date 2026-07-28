using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Discover a spell. Spend 2 Armor to Discover another."
// The second Discover is conditional, so
// EventCount stays 1 (only unconditional invocations count).
public class Jettison : ClassOrNeutralSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.Jettison;
}
