using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Give a minion +2/+2. Summon a random 2-Cost minion."
public class SilvermoonPortalKARA : Cost2MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.SilvermoonPortalKARA;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}

public class SilvermoonPortalCore : SilvermoonPortalKARA
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.SilvermoonPortalCore;
}

public class SilvermoonPortalWONDERS : SilvermoonPortalKARA
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.SilvermoonPortalWONDERS;
}
