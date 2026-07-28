using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Immune to Fire spells. Battlecry: Cast 15 Mana worth of Fire spells at random enemies."
// The number of casts is unpredictable (depends on rolled spell costs), so it is modeled
// as a single representative draw. Fire spell pool + generator inherited from Pyrotechnician.
public class FyrakkTheBlazing : FireSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.FyrakkTheBlazing;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
