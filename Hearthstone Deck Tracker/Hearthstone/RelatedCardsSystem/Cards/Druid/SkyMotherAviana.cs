
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Battlecry: Shuffle 10 random Legendary minions into your deck. They cost (1)."
public class SkyMotherAviana : LegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.SkyMotherAviana;
	public override int Picks() => 1;
	public override int EventCount() => 10;
	public override bool IsWithReplacement() => true;
}
