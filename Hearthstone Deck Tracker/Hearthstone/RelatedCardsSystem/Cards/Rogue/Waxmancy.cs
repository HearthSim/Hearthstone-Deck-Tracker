using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Discover a Battlecry minion. Reduce its Cost by (2)."
public class Waxmancy : ClassOrNeutralBattlecryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.Waxmancy;
}
