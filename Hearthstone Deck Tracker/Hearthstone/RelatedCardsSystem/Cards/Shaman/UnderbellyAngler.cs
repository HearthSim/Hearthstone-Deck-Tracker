using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "After you play a Murloc, add a random Murloc to your hand."
public class UnderbellyAngler : MurlocMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.UnderbellyAngler;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
