namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class HideousHusk: LeechGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.HideousHusk;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
