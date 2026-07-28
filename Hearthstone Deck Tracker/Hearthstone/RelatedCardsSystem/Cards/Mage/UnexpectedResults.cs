
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Summon two random 2-Cost minions (improved by Spell Damage)."
public class UnexpectedResults : Cost2MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.UnexpectedResults;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;
}
