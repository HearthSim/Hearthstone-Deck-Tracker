using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Discover a 3-Cost minion to summon. If your deck has no minions, repeat this."
// The repeat is conditional.
public class SpotTheDifference : ClassOrNeutralCost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.SpotTheDifference;
}
