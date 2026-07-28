
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Cast a random spell for each spell you've cast this game (targets chosen randomly)."
public class YoggSaronHopesEnd : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.YoggSaronHopesEnd;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	// Casts an unpredictable number of spells; model as a single representative draw.
	public override int EventCount() => 1;
}
