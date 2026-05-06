namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter.AnimalCompanionGenerator;

public class Spiritspeaker: AnimalCompanionGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.Spiritspeaker;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
