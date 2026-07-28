using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Kindred and Deathrattle: Get a random Deathrattle minion. It costs (2) less."
public class TrienniumRex : DeathrattleMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.TrienniumRex;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
