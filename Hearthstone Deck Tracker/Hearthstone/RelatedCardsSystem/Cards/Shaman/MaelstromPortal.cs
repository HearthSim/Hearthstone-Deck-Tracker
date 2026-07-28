using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Deal $1 damage to all enemy minions. Summon a random 1-Cost minion."
public class MaelstromPortal : Cost1MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.MaelstromPortal;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class MaelstromPortalCorePlaceholder : MaelstromPortal
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.MaelstromPortalCorePlaceholder;
}
