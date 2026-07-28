
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Battlecry: Discover a 1/1 copy of a Battlecry minion. It costs (1)."
public class GurubashiHypemon : ClassOrNeutralBattlecryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.GurubashiHypemon;
}
