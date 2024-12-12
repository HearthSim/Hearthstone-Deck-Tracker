namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class OnyxBishopKARA: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.OnyxBishopKARA;

	protected override bool FilterCard(Card card) => true;

	protected override bool ResurrectsMultipleCards() => false;
}
