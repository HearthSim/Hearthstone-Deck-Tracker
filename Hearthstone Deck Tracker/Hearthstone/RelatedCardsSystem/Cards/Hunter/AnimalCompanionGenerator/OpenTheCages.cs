namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter.AnimalCompanionGenerator;

public class OpenTheCages: AnimalCompanionGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.OpenTheCages;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
