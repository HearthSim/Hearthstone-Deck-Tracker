using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Battlecry: Discover a Legendary minion. Shuffle the other two into your deck."
public class MerchantOfLegend : ClassOrNeutralLegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.MerchantOfLegend;
}
