using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Add 2 random 1-Cost minions to your hand."
public class FirstDayOfSchool : Cost1MinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.FirstDayOfSchool;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
