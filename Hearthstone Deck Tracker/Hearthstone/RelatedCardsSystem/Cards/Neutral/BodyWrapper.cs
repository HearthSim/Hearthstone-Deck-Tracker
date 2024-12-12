namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class BodyWrapper: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.BodyWrapper;

	protected override bool FilterCard(Card card) => true;

	protected override bool ResurrectsMultipleCards() => false;
}
