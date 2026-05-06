namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter.AnimalCompanionGenerator;

public class CallOfTheWild: AnimalCompanionGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.CallOfTheWild;

	public bool ShouldShowForOpponent(Player opponent) => false;
}

public class CallOfTheWildCore: AnimalCompanionGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.CallOfTheWildCore;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
