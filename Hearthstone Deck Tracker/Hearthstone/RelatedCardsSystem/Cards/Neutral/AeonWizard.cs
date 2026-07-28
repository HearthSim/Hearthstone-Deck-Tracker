using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Rewind Battlecry: Get 2 random spells from your class."
public class AeonWizard : PlayerClassSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.AeonWizard;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
