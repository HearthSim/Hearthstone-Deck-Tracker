using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a Legendary minion with a Dark Gift."
public class TreacherousTormentor : ClassOrNeutralLegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.TreacherousTormentor;
}
