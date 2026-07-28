using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Deathrattle: Get a random card (from your opponent's class)."
// Known approximation: the real pool is the opponent's class specifically; the static
// cache can only express "any other class".
public class UndercityHucksterOG : OffClassCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.UndercityHucksterOG;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class UndercityHucksterCorePlaceholder : UndercityHucksterOG
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.UndercityHucksterCorePlaceholder;
}
