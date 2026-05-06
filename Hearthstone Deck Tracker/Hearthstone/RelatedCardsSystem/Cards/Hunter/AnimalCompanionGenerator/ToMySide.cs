namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter.AnimalCompanionGenerator;

public class ToMySide: AnimalCompanionGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.ToMySide;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
