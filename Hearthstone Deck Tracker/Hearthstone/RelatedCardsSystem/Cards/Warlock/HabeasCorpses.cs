namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class HabeasCorpses: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.HabeasCorpses;

	protected override bool FilterCard(Card card) => true;

	protected override bool ResurrectsMultipleCards() => false;
}
