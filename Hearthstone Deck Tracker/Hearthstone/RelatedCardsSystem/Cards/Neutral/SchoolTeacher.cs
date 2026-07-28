
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Add a 1/1 Nagaling to your hand. Discover a spell that costs (3) or less to teach it."
public class SchoolTeacher : ClassOrNeutralCostAtMost3SpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.SchoolTeacher;
}
