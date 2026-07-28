using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Freeze an enemy. Get a random Frost spell."
public class ColdSnap : FrostSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.ColdSnap;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
