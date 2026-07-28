using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: If you're holding a Dragon, Discover a Dragon."
public class NetherspiteHistorian : ClassOrNeutralDragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.NetherspiteHistorian;
}

public class NetherspiteHistorianCore : NetherspiteHistorian
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.NetherspiteHistorianCore;
}
