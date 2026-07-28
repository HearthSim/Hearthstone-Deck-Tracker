using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Deal $6 damage. Summon a random 6-Cost minion."
public class FirelandsPortal : Cost6MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.FirelandsPortal;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class FirelandsPortalCorePlaceholder : FirelandsPortal
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.FirelandsPortalCorePlaceholder;
}
