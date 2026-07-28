using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Add a random Mage spell to your hand."
public class BabblingBook : MageSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.BabblingBook;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class BabblingBookCorePlaceholder : BabblingBook
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.BabblingBookCorePlaceholder;
}
