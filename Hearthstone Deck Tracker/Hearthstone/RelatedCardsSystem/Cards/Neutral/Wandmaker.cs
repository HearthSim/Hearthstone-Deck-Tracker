using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Add a 1-Cost spell from your class to your hand."
public class Wandmaker : PlayerClassCost1SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Wandmaker;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class WandmakerCorePlaceholder : Wandmaker
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.WandmakerCorePlaceholder;
}
