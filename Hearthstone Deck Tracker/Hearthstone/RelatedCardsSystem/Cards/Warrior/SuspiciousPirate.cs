using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Battlecry: Discover a weapon. If your opponent guesses your choice, they get a copy."
public class SuspiciousPirate : ClassOrNeutralWeaponPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.SuspiciousPirate;
}

public class SuspiciousPirateCorePlaceholder : SuspiciousPirate
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.SuspiciousPirateCorePlaceholder;
}
