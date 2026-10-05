using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Discover a Deathrattle minion that costs (5) or more to summon."
// N'Zoth, Dream's End's ability.
public class CallOfNzoth : ClassOrNeutralCostAtLeast5DeathrattleMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.NZothDreamsEnd_CallOfNzothToken;
}
