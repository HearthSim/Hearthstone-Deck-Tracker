namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class XyrellaTheDevout: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.XyrellaTheDevout;

	protected override bool FilterCard(Card card) => card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => true;
}
