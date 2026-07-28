using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Deal $4 damage to a minion. Discover a spell."
public class MarkedShot : ClassOrNeutralSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.MarkedShot;
}

public class MarkedShotCorePlaceholder : MarkedShot
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.MarkedShotCorePlaceholder;
}
