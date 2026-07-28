using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Discover a spell. Your next one costs (1) less."
public class NatureStudies : ClassOrNeutralSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.NatureStudies;
}
