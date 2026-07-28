using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Discover a Fel spell. If you play it this turn, also pick one of the others."
public class HiveMap : ClassOrNeutralFelSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.HiveMap;
}
