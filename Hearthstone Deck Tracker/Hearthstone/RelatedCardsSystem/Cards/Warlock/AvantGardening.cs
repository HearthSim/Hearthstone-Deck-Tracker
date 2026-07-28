using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Discover a Deathrattle minion with a Dark Gift."
public class AvantGardening : ClassOrNeutralDeathrattleMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.AvantGardening;
}
