using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Gain 8 Armor. Summon a random 8-Cost minion."
public class CenarionWard : Cost8MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.CenarionWard;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
