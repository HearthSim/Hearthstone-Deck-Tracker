using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Discover a 3-Cost card. It costs (0)."
public class AzeriteChunk : ClassOrNeutralCost3CardPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_AzeriteChunkToken;
}
