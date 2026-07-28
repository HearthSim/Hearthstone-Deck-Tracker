using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Add a random weapon to your opponent's hand."
public class InstrumentCaseToken : WeaponPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.WorgenRoadie_InstrumentCaseToken;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
