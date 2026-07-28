using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "At the end of your turn, get a random Nature spell."
public class DaydreamingPixie : NatureSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.DaydreamingPixie;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
