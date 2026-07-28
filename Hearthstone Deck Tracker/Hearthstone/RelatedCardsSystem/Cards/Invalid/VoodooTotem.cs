using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Invalid;

// "At the end of your turn, get a random Shadow spell."
// Shadow spell pool + generator inherited from UmbralGeist.
public class VoodooTotem : ShadowSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Invalid.VoodooTotem;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
