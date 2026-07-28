using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Add a random Demon to your hand."
public class CallOfTheVoidLegacy : DemonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.CallOfTheVoidLegacy;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
