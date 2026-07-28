using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Discover a Choose One card. It has both effects combined. Give your opponent a plain copy."
public class NoxiousBribe : ClassOrNeutralChooseOneCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.NoxiousBribe;
}
