using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Rewind Battlecry: Discover a Beast with a Dark Gift. Kindred: It costs (1) less."
public class RaptorHeraldCore : ClassOrNeutralBeastMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.RaptorHeraldCore;
}
