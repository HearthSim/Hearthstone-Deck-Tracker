using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Discover a Temporary 1-Cost minion."
public class BloodpetalBiome : ClassOrNeutralCost1MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.BloodpetalBiome;
}
