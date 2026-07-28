using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Discover a Naga." — class + Neutral Nagas
public class OasisOutlaws : ClassOrNeutralNagaMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.OasisOutlaws;
}
