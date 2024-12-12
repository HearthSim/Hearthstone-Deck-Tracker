namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class RevivePet: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.RevivePet;

	protected override bool FilterCard(Card card) => card.IsBeast();

	protected override bool ResurrectsMultipleCards() => false;
}
