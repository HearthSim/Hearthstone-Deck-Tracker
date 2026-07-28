using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Cast 5 random spells (targets chosen randomly)."
public class YoggInTheBox : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.YoggInTheBox;
	public override int Picks() => 1;
	public override int EventCount() => 5;
	public override bool IsWithReplacement() => true;
}
