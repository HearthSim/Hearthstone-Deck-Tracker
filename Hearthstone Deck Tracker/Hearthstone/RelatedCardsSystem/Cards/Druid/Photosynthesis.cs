using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Restore 6 Health. Get 3 random Druid spells."
public class Photosynthesis : DruidSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.Photosynthesis;
	public override int Picks() => 1;
	public override int EventCount() => 3;
	public override bool IsWithReplacement() => true;
}
