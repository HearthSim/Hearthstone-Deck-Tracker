namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class TwilightsCall: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.TwilightsCall;

	protected override bool FilterCard(Card card) => card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => true;
}
