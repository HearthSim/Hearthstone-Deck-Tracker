namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Deal 1 damage to a minion. If it dies, Excavate a treasure."
public class Smokestack : ExcavateCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.Smokestack;
}
