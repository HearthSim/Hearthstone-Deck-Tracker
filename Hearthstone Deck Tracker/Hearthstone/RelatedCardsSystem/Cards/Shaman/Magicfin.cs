using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "After a friendly Murloc dies, add a random Legendary minion to your hand."
public class Magicfin : LegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Magicfin;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
