namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class EndbringerUmbra: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.EndbringerUmbra;

	protected override bool FilterCard(Card card) => card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => true;
}
