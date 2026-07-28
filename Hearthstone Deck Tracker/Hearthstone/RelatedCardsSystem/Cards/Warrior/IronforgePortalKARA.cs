using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Gain 4 Armor. Summon a random 4-Cost minion."
public class IronforgePortalKARA : Cost4MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.IronforgePortalKARA;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class IronforgePortalCore : IronforgePortalKARA
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.IronforgePortalCore;
}

public class IronforgePortalWONDERS : IronforgePortalKARA
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.IronforgePortalWONDERS;
}
