
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Choose a character. Cast 4 random spells (targeting it if possible)."
public class PrisonOfYoggSaron : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.PrisonOfYoggSaron;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 4;
}
