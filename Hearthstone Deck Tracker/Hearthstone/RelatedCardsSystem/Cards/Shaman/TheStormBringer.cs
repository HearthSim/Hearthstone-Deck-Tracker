
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Transform your minions into random Legendary minions."
public class TheStormBringer : LegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.TheStormBringer;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
