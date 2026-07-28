namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Add a random Legendary minion to your hand."
public class BrightwingLegacy : WeaponizedPinata
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.BrightwingLegacy;
}

public class BrightwingCore : BrightwingLegacy
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.BrightwingCore;
}
