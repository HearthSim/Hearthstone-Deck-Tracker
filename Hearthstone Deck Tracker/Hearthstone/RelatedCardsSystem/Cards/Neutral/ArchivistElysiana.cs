using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover 5 cards. Replace your deck with 2 copies of each."
public class ArchivistElysiana : ClassOrNeutralCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ArchivistElysiana;
	public override int EventCount() => 5;
}
