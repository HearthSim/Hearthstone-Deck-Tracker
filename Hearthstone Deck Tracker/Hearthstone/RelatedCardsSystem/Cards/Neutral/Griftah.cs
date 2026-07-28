using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover two cards. Give one to your opponent at random."
public class Griftah : ClassOrNeutralCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Griftah;
	public override int EventCount() => 2;
}
