using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Invalid;

// "Battlecry: Discover a 1-Cost minion with a Dark Gift."
public class BrutishEndmaw : ClassOrNeutralCost1MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Invalid.BrutishEndmaw;
}
