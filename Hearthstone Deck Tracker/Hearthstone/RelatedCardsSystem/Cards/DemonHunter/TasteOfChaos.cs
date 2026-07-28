using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Deal $2 damage to a minion. Finale: Discover a Fel spell."
public class TasteOfChaos : ClassOrNeutralFelSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.TasteOfChaos;
}
