
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Each turn this is in your hand, transform it into a random 4-Cost minion that gains Stealth."
public class MountHyjalImposter : Cost4MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.MountHyjalImposter;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
