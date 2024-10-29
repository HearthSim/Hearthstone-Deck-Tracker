namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class Headhunt: CrewmateGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.Headhunt;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
