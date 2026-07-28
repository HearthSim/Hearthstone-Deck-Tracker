using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Deal $2 damage. Discover a spell."
public class RunedOrb : ClassOrNeutralSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.RunedOrb;
}

public class RunedOrbCore : RunedOrb
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.RunedOrbCore;
}
