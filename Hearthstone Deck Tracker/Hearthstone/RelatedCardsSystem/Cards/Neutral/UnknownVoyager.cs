using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "After this survives damage, transform into a random 7-Cost minion."
public class UnknownVoyager : Cost7MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.UnknownVoyager;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
