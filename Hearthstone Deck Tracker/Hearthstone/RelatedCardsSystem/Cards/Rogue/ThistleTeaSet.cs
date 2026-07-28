using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Discover a spell from another class. Get a copy of it."
public class ThistleTeaSet : OffClassSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.ThistleTeaSet;
}
