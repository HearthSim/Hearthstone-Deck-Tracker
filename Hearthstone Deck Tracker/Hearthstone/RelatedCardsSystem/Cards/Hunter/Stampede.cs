using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Each time you play a Beast this turn, add a random Beast to your hand."
public class Stampede : BeastMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.Stampede;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
