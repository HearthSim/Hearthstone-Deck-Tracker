
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Each turn this is in your hand, transform it into a random 2-Cost minion that gains Spell Damage +1."
public class BlackMorassImposter : Cost2MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.BlackMorassImposter;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
