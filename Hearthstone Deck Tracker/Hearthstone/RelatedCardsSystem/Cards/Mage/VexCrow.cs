using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Whenever you cast a spell, summon a random 2-Cost minion."
public class VexCrow : Cost2MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.VexCrow;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
