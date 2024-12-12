namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class Boneshredder: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.Boneshredder;

	protected override bool FilterCard(Card card) => card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => false;
}
