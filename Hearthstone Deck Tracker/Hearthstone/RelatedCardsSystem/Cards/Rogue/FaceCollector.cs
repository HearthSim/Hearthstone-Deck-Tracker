
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Echo Battlecry: Add a random Legendary minion to your hand."
public class FaceCollector : LegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.FaceCollector;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
