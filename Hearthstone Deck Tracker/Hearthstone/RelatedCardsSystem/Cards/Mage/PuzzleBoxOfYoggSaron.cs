using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Cast 10 random spells."
public class PuzzleBoxOfYoggSaron : SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.PuzzleBoxOfYoggSaron;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 10;
}
