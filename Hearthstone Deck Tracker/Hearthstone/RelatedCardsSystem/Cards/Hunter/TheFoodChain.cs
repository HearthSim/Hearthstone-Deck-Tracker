using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Rush Battlecry: Discover any 8, 6, and 4-Attack Beast. Set their Costs to (2)."
public class ShokkJungleTyrantToken : Attack468BeastMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Hunter.TheFoodChain_ShokkJungleTyrantToken;
	public override int Picks() => 3;
}

public class TheFoodChain : Attack468BeastMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.TheFoodChain;
	public override int Picks() => 3;
}
