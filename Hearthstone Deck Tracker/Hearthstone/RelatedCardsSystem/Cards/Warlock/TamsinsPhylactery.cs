namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class TamsinsPhylactery: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.TamsinsPhylactery;

	protected override bool FilterCard(Card card) => card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => false;
}
