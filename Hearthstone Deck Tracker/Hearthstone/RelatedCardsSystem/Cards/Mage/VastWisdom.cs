using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Discover two spells that cost (3) or less. Swap their Costs."
public class VastWisdom : ClassOrNeutralCostAtMost3SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.VastWisdom;
	public override int Picks() => 3;
	public override int EventCount() => 2;
}
