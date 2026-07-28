using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Discover a 5-Cost minion. Spend 5 Corpses to summon a copy of it."
public class BloodClone : ClassOrNeutralCost5MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.BloodClone;
}
