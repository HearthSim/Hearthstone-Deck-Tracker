using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "After you play a Coin, get a random Legendary minion from another class and set its Cost to (1)."
public class WishingWell : OffClassLegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.WishingWell;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
