namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class EmergencyMeeting: CrewmateGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.EmergencyMeeting;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
