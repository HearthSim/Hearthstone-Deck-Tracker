using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "In 2 turns, summon three random Legendary minions. Get a 1-Cost copy of each."
// Kith'ix's Ritual.
public class RitualOfSubjugation : LegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Warlock.Kithix_RitualOfSubjugationToken;
	public override int Picks() => 1;
	public override int EventCount() => 3;
	public override bool IsWithReplacement() => true;
}
