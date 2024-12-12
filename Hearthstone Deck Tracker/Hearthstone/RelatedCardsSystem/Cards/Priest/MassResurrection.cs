namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class MassResurrection: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.MassResurrection;

	protected override bool FilterCard(Card card) => true;

	protected override bool ResurrectsMultipleCards() => true;
}
