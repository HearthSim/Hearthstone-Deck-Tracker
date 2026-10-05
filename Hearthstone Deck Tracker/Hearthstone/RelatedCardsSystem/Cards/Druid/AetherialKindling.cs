using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Deal 3 damage. Summon a random 3-Cost minion."
public class AetherialKindling : Cost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.AetherialKindling;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
