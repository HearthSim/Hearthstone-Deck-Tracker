
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Add a random 2-Cost minion to each player's hand."
public class TanglefurMystic : Cost2MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.TanglefurMystic;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 1;
}
