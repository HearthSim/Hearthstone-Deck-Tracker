using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Battlecry: Discover 2 Beasts. Put them on the bottom of your deck with +5/+5."
public class KaldoreiCultivator : ClassOrNeutralBeastMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.KaldoreiCultivator;
	public override int Picks() => 3;
	public override int EventCount() => 2;
}
