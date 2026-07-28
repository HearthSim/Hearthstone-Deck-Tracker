using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a spell. Forge: It costs (3) less."
public class MechagnomeGuide : ClassOrNeutralSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.MechagnomeGuide;
}

// "Forged Battlecry: Discover a spell. It costs (3) less."
public class MechagnomeGuideToken : MechagnomeGuide
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.MechagnomeGuide_MechagnomeGuideToken;
}
