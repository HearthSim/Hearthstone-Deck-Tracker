using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "After you play an Outcast card, add a random Outcast card to your hand."
public class WretchedExile : OutcastCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.WretchedExile;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
