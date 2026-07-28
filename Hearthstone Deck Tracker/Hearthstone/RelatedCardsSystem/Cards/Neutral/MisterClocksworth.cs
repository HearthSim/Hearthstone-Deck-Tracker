namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Rewind, Rewind, Rewind Battlecry: Summon 2 random Legendary minions."
public class MisterClocksworth : WeaponizedPinata
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.MisterClocksworth;
	public override int EventCount() => 2;
}
