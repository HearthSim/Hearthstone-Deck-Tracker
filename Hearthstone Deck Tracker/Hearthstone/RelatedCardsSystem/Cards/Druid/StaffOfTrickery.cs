using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "After your hero attacks, Discover a Druid card. Reduce its Cost by your hero's Attack."
public class StaffOfTrickery : DruidCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.StaffOfTrickery;
}
