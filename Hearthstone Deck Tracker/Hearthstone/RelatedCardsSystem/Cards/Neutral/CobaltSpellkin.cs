using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Add two 1-Cost spells from your class to your hand."
public class CobaltSpellkin : PlayerClassCost1SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.CobaltSpellkin;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
