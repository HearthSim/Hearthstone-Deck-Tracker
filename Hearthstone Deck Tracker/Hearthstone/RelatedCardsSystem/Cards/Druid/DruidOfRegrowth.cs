using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Rewind. Battlecry: Cast 2 random Nature spells."
public class DruidOfRegrowth : NatureSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.DruidOfRegrowth;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
