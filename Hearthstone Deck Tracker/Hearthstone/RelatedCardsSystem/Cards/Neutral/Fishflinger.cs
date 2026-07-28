using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Add a random Murloc to each player's hand."
public class Fishflinger : MurlocMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Fishflinger;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
