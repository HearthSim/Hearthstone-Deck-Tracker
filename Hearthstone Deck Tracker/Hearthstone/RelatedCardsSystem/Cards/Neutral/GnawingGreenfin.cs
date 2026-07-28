using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Get a random Murloc."
public class GnawingGreenfin : MurlocMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.GnawingGreenfin;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
