using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "At the start of your turn, Discover a Dragon. It costs (4) less."
public class PurifiedDragonNestToken : ClassOrNeutralDragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Druid.Rheastrasza_PurifiedDragonNestToken;
}
