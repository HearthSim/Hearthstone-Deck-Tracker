namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter.AnimalCompanionGenerator;

public class AnimalCompanion: AnimalCompanionGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.AnimalCompanionCore;

	public bool ShouldShowForOpponent(Player opponent) => false;
}

public class AnimalCompanionLegacy: AnimalCompanionGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.AnimalCompanionLegacy;

	public bool ShouldShowForOpponent(Player opponent) => false;
}

public class AnimalCompanionVanilla: AnimalCompanionGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.AnimalCompanionVanilla;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
