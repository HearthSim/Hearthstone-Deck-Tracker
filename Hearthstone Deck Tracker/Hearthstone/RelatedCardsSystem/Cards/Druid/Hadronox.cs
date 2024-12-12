namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class Hadronox: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.Hadronox;

	protected override bool FilterCard(Card card) => card.HasTaunt();

	protected override bool ResurrectsMultipleCards() => true;
}
