using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Discover a 6-Cost minion. Summon two copies of it."
public class PowerOfCreation : ClassOrNeutralCost6MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.PowerOfCreation;
}
