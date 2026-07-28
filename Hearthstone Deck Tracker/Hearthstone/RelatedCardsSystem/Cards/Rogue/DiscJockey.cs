using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Combo: Add a random Combo card to your hand."
public class DiscJockey : ComboCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.DiscJockey;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
