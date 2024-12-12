namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class AmuletOfUndying: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.AmuletOfUndying;

	protected override bool FilterCard(Card card) => card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => true;
}
