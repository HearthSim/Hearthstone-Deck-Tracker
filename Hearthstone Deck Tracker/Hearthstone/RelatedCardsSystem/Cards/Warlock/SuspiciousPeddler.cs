using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Battlecry: Discover a 1-Cost card. If your opponent guesses your choice, they get a copy."
public class SuspiciousPeddler : ClassOrNeutralCost1CardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.SuspiciousPeddler;
}
