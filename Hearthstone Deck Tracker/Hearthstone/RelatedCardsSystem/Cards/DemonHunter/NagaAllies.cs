using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Discover a Naga minion."
public class NagaAllies : ClassOrNeutralNagaMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.NagaAllies;
}
