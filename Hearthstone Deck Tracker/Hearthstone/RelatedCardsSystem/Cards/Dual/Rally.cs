namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Dual;

public class Rally: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Rally;

	protected override bool FilterCard(Card card) => card.Attack is >= 1 and <= 3;

	protected override bool ResurrectsMultipleCards() => true;
}
