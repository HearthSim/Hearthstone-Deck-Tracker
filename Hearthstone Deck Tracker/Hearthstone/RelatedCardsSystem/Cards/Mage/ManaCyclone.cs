namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: For each spell you've cast this turn, add a random Mage spell to your hand."
public class ManaCyclone : BabblingBook
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.ManaCyclone;
}
