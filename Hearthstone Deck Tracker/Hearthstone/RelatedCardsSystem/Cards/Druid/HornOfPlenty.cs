using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Discover a Nature spell. It costs (2) less."
public class HornOfPlenty : ClassOrNeutralNatureSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.HornOfPlenty;
}
