
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Taunt Battlecry: Add a random 4-Cost minion to your opponent's hand."
public class KthirRitualist : Cost4MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.KthirRitualist;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
