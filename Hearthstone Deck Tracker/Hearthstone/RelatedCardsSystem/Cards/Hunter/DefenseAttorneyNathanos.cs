namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class DefenseAttorneyNathanos: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.DefenseAttorneyNathanos;

	protected override bool FilterCard(Card card) => card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => false;
}
