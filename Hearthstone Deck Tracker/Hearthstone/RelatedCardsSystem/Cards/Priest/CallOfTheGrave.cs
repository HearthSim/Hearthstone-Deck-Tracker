using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Discover a Deathrattle minion. If you have enough Mana to play it, trigger its Deathrattle."
public class CallOfTheGrave : ClassOrNeutralDeathrattleMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.CallOfTheGrave;
}
