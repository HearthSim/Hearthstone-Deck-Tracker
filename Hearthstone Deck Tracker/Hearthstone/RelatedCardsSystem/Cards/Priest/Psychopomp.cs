namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class Psychopomp: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.Psychopomp;

	protected override bool FilterCard(Card card) => true;

	protected override bool ResurrectsMultipleCards() => false;
}
