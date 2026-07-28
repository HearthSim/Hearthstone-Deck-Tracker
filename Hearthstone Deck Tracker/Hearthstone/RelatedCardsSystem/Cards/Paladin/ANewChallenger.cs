using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Discover a 6-Cost minion. Summon it with Taunt and Divine Shield."
public class ANewChallenger : ClassOrNeutralCost6MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.ANewChallenger;
}
