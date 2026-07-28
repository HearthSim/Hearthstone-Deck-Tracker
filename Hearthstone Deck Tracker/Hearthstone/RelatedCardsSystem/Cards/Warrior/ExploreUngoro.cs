
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Replace your deck with copies of "Discover a card.""
public class ExploreUngoro : ClassOrNeutralCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.ExploreUngoro;
}
