using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a Legendary minion."
public class Paparazzi : ClassOrNeutralLegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Paparazzi;
}
