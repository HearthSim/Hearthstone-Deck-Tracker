namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class DirdraRebelCaptain: CrewmateGenerator, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.DirdraRebelCaptain;

	public bool ShouldShowForOpponent(Player opponent) => false;
}
