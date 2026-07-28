using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Whenever you draw a card, transform it into a random Legendary minion."
public class Transmogrifier : LegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Transmogrifier;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
