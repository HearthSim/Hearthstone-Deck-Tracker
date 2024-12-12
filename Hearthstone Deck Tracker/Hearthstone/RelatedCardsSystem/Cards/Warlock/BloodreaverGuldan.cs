namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class BloodreaverGuldan: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.BloodreaverGuldan;

	protected override bool FilterCard(Card card) => card.IsDemon();

	protected override bool ResurrectsMultipleCards() => true;
}
