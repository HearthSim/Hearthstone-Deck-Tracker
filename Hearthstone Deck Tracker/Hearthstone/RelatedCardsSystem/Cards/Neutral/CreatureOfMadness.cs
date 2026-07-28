using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a 3-Cost minion with a Dark Gift."
public class CreatureOfMadness : ClassOrNeutralCost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.CreatureOfMadness;
}
