namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class InfestedBreath: LeechGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.InfestedBreath;

	public bool ShouldShowForOpponent(Player opponent) => false;
}


public class InfestedBreatsh: LeechGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.WhelpOfTheInfinite;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
