using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Discover a Deathrattle card."
public class JourneyBelow : ClassOrNeutralDeathrattleCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.JourneyBelow;
}
