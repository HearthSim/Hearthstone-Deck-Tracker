using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Deal $3 damage. Get a random spell. Forge: This casts twice."
public class MoltenRune : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.MoltenRune;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
