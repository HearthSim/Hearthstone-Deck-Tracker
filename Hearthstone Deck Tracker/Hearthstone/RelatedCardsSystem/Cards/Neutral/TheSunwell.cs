
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Fill your hand with random spells. Costs (1) less for each other card in your hand."
public class TheSunwell : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.TheSunwell;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	// Fills the hand with an unpredictable number of spells; model as a single representative draw.
	public override int EventCount() => 1;
}
