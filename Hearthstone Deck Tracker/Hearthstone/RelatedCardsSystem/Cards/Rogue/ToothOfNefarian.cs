using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Deal 3 damage. Honorable Kill: Discover a spell from another class."
public class ToothOfNefarian : OffClassSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.ToothOfNefarian;
}
