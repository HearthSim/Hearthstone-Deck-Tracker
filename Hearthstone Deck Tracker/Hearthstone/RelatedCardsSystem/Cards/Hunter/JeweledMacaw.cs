using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Battlecry: Add a random Beast to your hand."
public class JeweledMacaw : BeastMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.JeweledMacaw;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class JeweledMacawCore : JeweledMacaw
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.JeweledMacawCore;
}
