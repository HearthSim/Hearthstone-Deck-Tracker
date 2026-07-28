using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "After you play a Beast, add a random Beast to your hand."
public class Saddlemaster : BeastMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.Saddlemaster;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
