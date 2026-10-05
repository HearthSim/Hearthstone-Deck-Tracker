using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Discover a spell to put on top of your deck. It costs (2) less."
public class Coilbearer : ClassOrNeutralSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.Coilbearer;
}
