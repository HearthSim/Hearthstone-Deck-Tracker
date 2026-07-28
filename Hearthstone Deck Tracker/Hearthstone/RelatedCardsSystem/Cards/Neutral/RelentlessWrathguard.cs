
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Deal 2 damage to an enemy minion. If it dies, Discover a Demon."
public class RelentlessWrathguard : ClassOrNeutralDemonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.RelentlessWrathguard;
}
