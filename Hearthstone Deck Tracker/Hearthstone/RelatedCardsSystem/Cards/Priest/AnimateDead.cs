namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class AnimateDead: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.AnimateDead;

	protected override bool FilterCard(Card card) => card.Cost <= 3;

	protected override bool ResurrectsMultipleCards() => false;
}
