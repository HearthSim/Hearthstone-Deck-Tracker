namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class NzothGodOfTheDeep: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.NzothGodOfTheDeep;

	protected override bool FilterCard(Card card) => true;

	protected override bool ResurrectsMultipleCards() => true;
}
