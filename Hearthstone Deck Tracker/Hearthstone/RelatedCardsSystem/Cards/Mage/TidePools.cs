using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Discover a spell that costs (3) or less. After you cast a spell, reopen this."
public class TidePools : ClassOrNeutralCostAtMost3SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.TidePools;
}
