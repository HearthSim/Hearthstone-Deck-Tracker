namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class InfantryReanimator: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.InfantryReanimator;

	protected override bool FilterCard(Card card) => card.IsUndead();

	protected override bool ResurrectsMultipleCards() => false;
}
