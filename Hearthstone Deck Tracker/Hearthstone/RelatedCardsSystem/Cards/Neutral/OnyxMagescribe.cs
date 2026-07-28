using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Spellburst: Add 2 random spells from your class to your hand."
public class OnyxMagescribe : PlayerClassSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.OnyxMagescribe;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
