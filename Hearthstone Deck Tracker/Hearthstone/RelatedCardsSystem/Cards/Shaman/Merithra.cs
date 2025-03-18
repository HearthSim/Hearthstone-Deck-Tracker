namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class Merithra: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Merithra;

	protected override bool FilterCard(Card card) => card.Cost >= 8;

	protected override bool ResurrectsMultipleCards() => false;
}
