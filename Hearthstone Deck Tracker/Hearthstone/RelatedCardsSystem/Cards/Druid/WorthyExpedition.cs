using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Discover a Choose One card."
public class WorthyExpedition : ClassOrNeutralChooseOneCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.WorthyExpedition;
}
