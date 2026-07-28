using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Whenever you restore Health, add a random Druid spell to your hand."
public class Lifeweaver : DruidSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.Lifeweaver;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
