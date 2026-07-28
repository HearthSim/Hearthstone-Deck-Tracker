using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Add 2 random spells from another class to your hand."
public class CleverDisguise : OffClassSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.CleverDisguise;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;
}
