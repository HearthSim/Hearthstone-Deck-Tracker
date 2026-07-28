using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Give your minions 'Deathrattle: Add a random Mech to your hand.'"
public class CybertechChip : MechMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.CybertechChip;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
