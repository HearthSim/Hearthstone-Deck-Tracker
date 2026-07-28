namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Discover a spell. If you have enough Mana to play it, cast a copy of it at a random enemy."
public class VoidScripture : RunedOrb
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.VoidScripture;
}
