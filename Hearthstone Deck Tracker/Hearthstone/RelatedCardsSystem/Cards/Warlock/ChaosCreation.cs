using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Deal $6 damage. Summon a random 6-Cost minion. Destroy the bottom 6 cards of your deck."
public class ChaosCreation : Cost6MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.ChaosCreation;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
