using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a weapon from any class."
public class RunefueledGolem : WeaponPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.RunefueledGolem;

	// Discover (not a single random draw), so 3 unique choices from the full weapon pool.
	public override int Picks() => 3;
	public override bool IsWithReplacement() => false;
}
