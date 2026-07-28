using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Discover two 8-Cost minions to summon with Taunt and Elusive."
public class Nebula : ClassOrNeutralCost8MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Nebula;
	public override int EventCount() => 2;
}
