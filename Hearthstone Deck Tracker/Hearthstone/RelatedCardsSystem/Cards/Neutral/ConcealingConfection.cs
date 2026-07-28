using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Get a random weapon."
public class ConcealingConfection : WeaponPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ConcealingConfection;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
