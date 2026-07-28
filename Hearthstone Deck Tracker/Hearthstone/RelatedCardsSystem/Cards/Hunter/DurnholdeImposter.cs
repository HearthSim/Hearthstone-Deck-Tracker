
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Each turn this is in your hand, transform it into a random 3-Cost minion that gains Poisonous."
public class DurnholdeImposter : Cost3MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.DurnholdeImposter;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
