using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Give a minion +2/+2. If it's an Elemental, add a random Elemental to your hand."
public class EarthenMight : ElementalMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Shaman.EarthenMight;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
