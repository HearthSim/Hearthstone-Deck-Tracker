using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Shuffle 10 cards from your opponent's class into your deck. They cost (1)."
// Known approximation: the real pool is the opponent's class specifically; the static
// cache can only express "any other class".
public class AcademicEspionage : OffClassCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.AcademicEspionage;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 10;
}
