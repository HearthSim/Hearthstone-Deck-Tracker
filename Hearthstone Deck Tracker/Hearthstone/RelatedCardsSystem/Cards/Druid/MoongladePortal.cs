using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Restore 6 Health. Summon a random 6-Cost minion."
public class MoongladePortal : Cost6MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.MoongladePortal;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
