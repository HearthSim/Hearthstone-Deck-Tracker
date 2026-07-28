namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Deal $2 damage to all minions. Overkill: Add a random Mage spell to your hand."
public class BlastWave : BabblingBook
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.BlastWave;
}
