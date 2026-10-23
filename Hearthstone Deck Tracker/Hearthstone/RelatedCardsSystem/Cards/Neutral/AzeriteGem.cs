using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Discover a 5-Cost card. It costs (0)."
public class AzeriteGem : ClassOrNeutralCost5CardPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_AzeriteGemToken;
}
