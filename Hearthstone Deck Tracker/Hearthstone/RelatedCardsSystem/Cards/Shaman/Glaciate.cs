using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Discover an 8-Cost minion. Summon and Freeze it."
public class Glaciate : ClassOrNeutralCost8MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Glaciate;
}

public class GlaciateCore : Glaciate
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.GlaciateCore;
}
