namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class SanguineInfestation: LeechGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.SanguineInfestation;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
