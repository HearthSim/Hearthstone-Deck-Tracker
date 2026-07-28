using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Discover a 3-Cost minion. Summon a 2/3 copy of it."
public class RitualOfLife : ClassOrNeutralCost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.RitualOfLife;
}
