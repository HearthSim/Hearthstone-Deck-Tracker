using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Taunt Battlecry: Discover a Taunt minion."
public class FrightenedFlunky : ClassOrNeutralTauntMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.FrightenedFlunky;
}

public class FrightenedFlunkyCorePlaceholder : FrightenedFlunky
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.FrightenedFlunkyCorePlaceholder;
}
