using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "After you cast a Fire spell, get a random Elemental and reduce its Cost by (3)."
public class InfernoHerald : ElementalMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.InfernoHerald;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
