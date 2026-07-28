
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Destroy a minion. Add a random minion to your opponent's hand."
public class Mulch : MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.Mulch;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
