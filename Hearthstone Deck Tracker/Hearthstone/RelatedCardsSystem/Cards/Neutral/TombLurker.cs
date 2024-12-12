namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class TombLurker: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.TombLurker;

	protected override bool FilterCard(Card card) => card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => false;
}
