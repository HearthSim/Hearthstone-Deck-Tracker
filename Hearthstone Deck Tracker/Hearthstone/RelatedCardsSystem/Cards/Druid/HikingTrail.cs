using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Discover a Taunt minion. After you gain Armor, reopen this."
public class HikingTrail : ClassOrNeutralTauntMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.HikingTrail;
}
