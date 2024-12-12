namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class CounterfeitBlade: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.CounterfeitBlade;

	protected override bool FilterCard(Card card) => card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => false;
}
