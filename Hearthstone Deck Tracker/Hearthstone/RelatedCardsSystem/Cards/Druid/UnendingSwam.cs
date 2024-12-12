namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class UnendingSwarm: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.UnendingSwarm;

	protected override bool FilterCard(Card card) => card.Cost <= 2;

	protected override bool ResurrectsMultipleCards() => true;
}
