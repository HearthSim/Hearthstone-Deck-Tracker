namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class AnyfinCanHappen: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.AnyfinCanHappen;

	protected override bool FilterCard(Card card) => card.IsMurloc();

	protected override bool ResurrectsMultipleCards() => true;
}
