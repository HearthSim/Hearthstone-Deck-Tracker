using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Discover a Legendary minion. Summon two copies of it."
public class ZarogsCrownToken : LegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.MarintheManager_ZarogsCrownToken;
}
