
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Battlecry and Inspire: Summon a random Legendary minion."
public class ConfessorPaletressTGT : LegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.ConfessorPaletressTGT;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class ConfessorPaletressWONDERS : ConfessorPaletressTGT
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.ConfessorPaletressWONDERS;
}
