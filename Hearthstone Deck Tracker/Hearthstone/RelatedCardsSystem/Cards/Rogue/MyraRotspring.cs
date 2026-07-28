
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Battlecry: Discover a Deathrattle minion. Also gain its Deathrattle."
public class MyraRotspring : ClassOrNeutralDeathrattleMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.MyraRotspring;
}
