namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Dual;

public class RaiseDead: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.RaiseDead;

	protected override bool FilterCard(Card card) => true;

	protected override bool ResurrectsMultipleCards() => true;
}
