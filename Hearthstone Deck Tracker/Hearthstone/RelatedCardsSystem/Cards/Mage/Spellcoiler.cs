namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: If you've cast a spell while holding this, Discover a spell."
public class Spellcoiler : RunedOrb
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.Spellcoiler;
}
