using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "After this minion survives damage, summon a random Legendary minion."
public class Rotface : LegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.Rotface;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class RotfaceCorePlaceholder : Rotface
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.RotfaceCorePlaceholder;
}
