namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class CatrinaMuerte: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.CatrinaMuerte;

	protected override bool FilterCard(Card card) => card.Id != GetCardId() && card.IsUndead();

	protected override bool ResurrectsMultipleCards() => false;
}
