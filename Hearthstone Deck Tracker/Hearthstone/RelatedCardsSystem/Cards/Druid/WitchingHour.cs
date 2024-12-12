namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class WitchingHour: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.WitchingHour;

	protected override bool FilterCard(Card card) => card.IsBeast();

	protected override bool ResurrectsMultipleCards() => false;
}
