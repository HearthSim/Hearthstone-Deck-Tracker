
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Transform minions in your deck into random Dragons. (They keep their original stats and Cost.)"
public class LadyPrestor : DragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.LadyPrestor;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	// Transforms an unpredictable number of deck minions; model as a single representative draw.
	public override int EventCount() => 1;
}
