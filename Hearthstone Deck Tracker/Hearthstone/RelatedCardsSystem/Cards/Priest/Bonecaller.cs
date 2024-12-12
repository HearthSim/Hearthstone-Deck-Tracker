namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class Bonecaller: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.Bonecaller;

	protected override bool FilterCard(Card card) => card.IsUndead();

	protected override bool ResurrectsMultipleCards() => false;
}
