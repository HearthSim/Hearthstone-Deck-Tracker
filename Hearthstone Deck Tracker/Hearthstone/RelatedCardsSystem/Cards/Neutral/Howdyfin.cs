using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Whenever your hand has less than 3 cards in it, get a random Murloc."
public class Howdyfin : MurlocMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Howdyfin;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
