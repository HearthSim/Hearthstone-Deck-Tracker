using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "At the start of your turn, transform into a random 5-Cost minion."
public class DangerousVariant : Cost5MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.DangerousVariant;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
