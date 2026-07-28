
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Each turn this is in your hand, transform it into a random minion."
public class ShifterZerus : MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ShifterZerus;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
