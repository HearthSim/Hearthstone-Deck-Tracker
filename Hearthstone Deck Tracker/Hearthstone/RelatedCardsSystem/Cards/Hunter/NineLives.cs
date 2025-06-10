namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class NineLives: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.NineLives;

	protected override bool FilterCard(Card card) => card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => false;
}
