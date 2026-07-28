using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Get 3 random cards (from your opponent's class)."
// Another-class pool inherited from Swashburglar. Known approximation: the real pool is
// the opponent's class specifically; the static cache can only express "any other class".
public class BurgleTGT : OffClassCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.BurgleTGT;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 3;
}

public class BurgleWONDERS : BurgleTGT
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.BurgleWONDERS;
}
