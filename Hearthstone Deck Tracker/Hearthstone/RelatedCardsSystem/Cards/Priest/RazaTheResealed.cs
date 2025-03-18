namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class RazaTheResealed: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.RazaTheResealed;

	protected override bool FilterCard(Card card) => true;

	protected override bool ResurrectsMultipleCards() => true;
}
