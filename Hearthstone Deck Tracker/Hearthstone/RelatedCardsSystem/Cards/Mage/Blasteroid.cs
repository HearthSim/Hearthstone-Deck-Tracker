using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Shuffle 5 random Fire spells into your deck. They cost (2) less."
// Fire spell pool + generator inherited from Pyrotechnician.
public class Blasteroid : FireSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.Blasteroid;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 5;
}
