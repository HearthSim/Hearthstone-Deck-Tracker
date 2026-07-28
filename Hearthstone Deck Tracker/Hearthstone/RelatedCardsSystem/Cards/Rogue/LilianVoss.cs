using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Battlecry: Replace spells in your hand with random spells (from your opponent's class)."
// Another-class spell pool inherited from HenchClanBurglar. Known approximations: the real
// pool is the opponent's class specifically, and the replaced count is unpredictable, so
// this is modeled as a single representative draw.
public class LilianVoss : OffClassSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.LilianVoss;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class LilianVossCorePlaceholder : LilianVoss
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.LilianVossCorePlaceholder;
}
