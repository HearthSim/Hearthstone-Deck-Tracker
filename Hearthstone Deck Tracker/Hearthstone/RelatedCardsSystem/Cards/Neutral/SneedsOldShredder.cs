namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Summon a random Legendary minion."
public class SneedsOldShredder : WeaponizedPinata
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.SneedsOldShredder;
}

public class SneedsOldShredderCore : SneedsOldShredder
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.SneedsOldShredderCore;
}
