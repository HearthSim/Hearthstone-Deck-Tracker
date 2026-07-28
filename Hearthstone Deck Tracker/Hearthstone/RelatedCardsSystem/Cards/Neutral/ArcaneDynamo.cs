using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a spell that costs (5) or more."
public class ArcaneDynamo : ClassOrNeutralCostAtLeast5SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ArcaneDynamo;
}
