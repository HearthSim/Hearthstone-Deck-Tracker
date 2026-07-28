using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Discover a 5-Cost minion. Summon it with +1/+1. (Swaps each turn.)"
public class HarmonicDisco : ClassOrNeutralCost5MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.HarmonicDisco;
}

// "Discover a 1-Cost minion. Summon it with +5/+5. (Swaps each turn.)"
public class HarmonicDiscoSwapped : ClassOrNeutralCost1MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Paladin.HarmonicDisco_DissonantDiscoToken;
}
