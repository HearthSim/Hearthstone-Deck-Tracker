using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Discover a spell that costs (3) or less."
public class MagicTrick : ClassOrNeutralCostAtMost3SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.MagicTrick;
}
