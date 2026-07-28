
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Choose One - Get two random Dragons that cost (5) or less; or Get two that cost more than (5)."
public class DragonTales : DragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.DragonTales;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;
}
