namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class RavenousFelhunter: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.RavenousFelhunter;

	protected override bool FilterCard(Card card) => card.Cost <= 4;

	protected override bool ResurrectsMultipleCards() => false;
}
