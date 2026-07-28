using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Secretly Discover a Dragon to hatch into. Deathrattle: Hatch!"
public class ChromaticEgg : ClassOrNeutralDragonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ChromaticEgg;
}
