namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Fill your hand with random Mage spells. They are Temporary."
public class Evocation : BabblingBook
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.Evocation;
	public override bool IsWithReplacement() => true;
}
