using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Give your minions 'Deathrattle: Add a random Beast to your hand.'"
public class Infest : BeastMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.Infest;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
