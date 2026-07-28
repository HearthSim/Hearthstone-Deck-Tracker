using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a card. Reduce its Cost by (1)."
public class DwarvenArchaeologist : ClassOrNeutralCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.DwarvenArchaeologist;
}
