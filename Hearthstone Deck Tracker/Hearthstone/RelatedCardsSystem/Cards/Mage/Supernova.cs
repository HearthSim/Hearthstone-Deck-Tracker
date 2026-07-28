using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Fill your hand with random Fire spells. They cost (1)."
// Hand-fill count is unpredictable, so it is modeled as a single representative draw.
// Fire spell pool + generator inherited from Pyrotechnician.
public class Supernova : FireSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.Supernova;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
