namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class JewelOfNzoth: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.JewelOfNzoth;

	protected override bool FilterCard(Card card) => card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => true;
}
