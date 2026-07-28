using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Give a friendly minion 'Deathrattle: Summon two random 4-Cost minions.'"
public class DigForFreedom : Cost4MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.DigForFreedom;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
