using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Get a random playable spell. It is Temporary."
// All-spells pool inherited from YoggInTheBox. Known approximation: "playable" depends on
// remaining mana (live state), which a static pool cannot express — the pool shows every
// spell regardless of cost. Temporary is a post-pick modifier.
public class FranticForger : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.FranticForger;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
