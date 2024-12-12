namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class StranglethornHeart: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.StranglethornHeart;

	protected override bool FilterCard(Card card) => card.IsBeast() && card.Cost >= 5;

	protected override bool ResurrectsMultipleCards() => true;
}
