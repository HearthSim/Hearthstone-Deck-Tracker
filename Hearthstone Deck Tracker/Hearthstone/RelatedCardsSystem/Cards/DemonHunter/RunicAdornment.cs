using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Discover a spell that costs (3) or less. Shuffle 2 copies into your deck that Cast When Drawn."
public class RunicAdornment : ClassOrNeutralCostAtMost3SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.RunicAdornment;
}
