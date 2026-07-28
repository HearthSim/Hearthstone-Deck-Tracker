using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Deathrattle: Add a random Outcast card to your hand."
public class CalamitysGrasp : OutcastCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.CalamitysGrasp;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
