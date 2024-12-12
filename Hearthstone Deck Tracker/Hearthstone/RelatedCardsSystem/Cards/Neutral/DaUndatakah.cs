namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class DaUndatakah: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.DaUndatakah;

	protected override bool FilterCard(Card card) => card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => true;
}
