using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Rewind Discover a spell from ANY class. (Or just your class after you Rewind!)"
// Technically the first pick of Sands of Time is any spell, but it is more useful to show
// the class-specific pool, as it also helps with the Rewind decision. Spell pool inherited
// from Astrobiologist.
public class SandsOfTime : ClassOrNeutralSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.SandsOfTime;
}
