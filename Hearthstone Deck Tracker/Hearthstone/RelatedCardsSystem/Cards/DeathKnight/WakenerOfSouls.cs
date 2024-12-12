namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class WakenerOfSouls: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.WakenerOfSouls;

	protected override bool FilterCard(Card card) => card.Id != GetCardId() && card.HasDeathrattle();

	protected override bool ResurrectsMultipleCards() => false;
}
