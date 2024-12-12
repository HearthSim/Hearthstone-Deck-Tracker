namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class Vectus: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Vectus;

	protected override bool FilterCard(Card card) => card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => true;
}
