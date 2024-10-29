namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class VoroneiRecruiter: CrewmateGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.VoroneiRecruiter;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
