using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Discover a Deathrattle minion. Your next one costs (1) less."
public class CarrionStudies : ClassOrNeutralDeathrattleMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.CarrionStudies;
}
