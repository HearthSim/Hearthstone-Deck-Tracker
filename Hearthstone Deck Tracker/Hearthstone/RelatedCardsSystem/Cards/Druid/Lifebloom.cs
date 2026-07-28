using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Restore 8 Health to all friendly characters. Summon two random 8-Cost minions."
public class Lifebloom : Cost8MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.Lifebloom;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;
}
