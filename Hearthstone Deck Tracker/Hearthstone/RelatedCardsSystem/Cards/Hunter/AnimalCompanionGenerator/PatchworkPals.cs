namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter.AnimalCompanionGenerator;

public class PatchworkPals: AnimalCompanionGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.PatchworkPals;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
