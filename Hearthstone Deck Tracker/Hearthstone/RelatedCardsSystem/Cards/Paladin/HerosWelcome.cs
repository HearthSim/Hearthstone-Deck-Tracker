using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Discover a Legendary minion to summon. Set its stats to 10/10."
public class HerosWelcome : ClassOrNeutralLegendaryMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.HerosWelcome;
}
