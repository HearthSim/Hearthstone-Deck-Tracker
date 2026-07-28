using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Summon two random 2-Cost minions. Refresh 2 Mana Crystals."
public class DistressSignal : Cost2MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.DistressSignal;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;
}
