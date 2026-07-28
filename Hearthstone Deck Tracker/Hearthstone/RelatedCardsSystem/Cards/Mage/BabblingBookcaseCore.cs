using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Add 2 random Mage spells to your hand."
public class BabblingBookcaseCore : MageSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.BabblingBookcaseCore;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;
}
