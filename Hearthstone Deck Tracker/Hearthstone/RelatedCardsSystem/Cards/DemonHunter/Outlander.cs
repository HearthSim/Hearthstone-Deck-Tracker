using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "If you played an Outcast card this turn, Discover a Fel Spell."
public class Outlander : ClassOrNeutralFelSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.Outlander;
}
