using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Add a random card from another class to your hand."
public class PilferLegacy : OffClassCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.PilferLegacy;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
