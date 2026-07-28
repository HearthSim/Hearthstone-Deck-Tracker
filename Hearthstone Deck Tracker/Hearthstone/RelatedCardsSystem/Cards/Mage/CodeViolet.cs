using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Prepare. Summon an 8-Cost minion. If you've cast 3 other spells this turn, do it again."
// The repeat is conditional, so EventCount stays 1.
public class CodeViolet : Cost8MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.CodeViolet;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
