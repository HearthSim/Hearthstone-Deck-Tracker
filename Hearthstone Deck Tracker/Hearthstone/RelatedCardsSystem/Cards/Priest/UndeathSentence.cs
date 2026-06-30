namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class UndeathSentence: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.UndeathSentence;

	protected override bool FilterCard(Card card) => card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => false;
}
