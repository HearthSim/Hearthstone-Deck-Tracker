namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class EternalServitude: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.EternalServitude;

	protected override bool FilterCard(Card card) => true;

	protected override bool ResurrectsMultipleCards() => false;
}
