namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Discover a spell. Choose to keep it or put it on top of your opponent's deck."
public class Qonzu : RunedOrb
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.Qonzu;
}
