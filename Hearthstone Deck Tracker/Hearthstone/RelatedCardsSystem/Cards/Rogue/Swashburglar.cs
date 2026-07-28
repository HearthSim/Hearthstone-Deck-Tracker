using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Battlecry: Add a random card from another class to your hand."
public class Swashburglar : OffClassCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.Swashburglar;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class SwashburglarCore : Swashburglar
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.SwashburglarCore;
}
