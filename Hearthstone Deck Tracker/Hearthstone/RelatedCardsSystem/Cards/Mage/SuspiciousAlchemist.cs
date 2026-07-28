namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Discover a spell. If your opponent guesses your choice, they get a copy."
public class SuspiciousAlchemist : RunedOrb
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.SuspiciousAlchemist;
}

public class SuspiciousAlchemistCorePlaceholder : SuspiciousAlchemist
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.SuspiciousAlchemistCorePlaceholder;
}
