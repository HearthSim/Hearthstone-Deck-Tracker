using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Discover a spell that costs (5) or more. It casts twice when played."
public class BreakoutArchitect : ClassOrNeutralCostAtLeast5SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.BreakoutArchitect;
}
