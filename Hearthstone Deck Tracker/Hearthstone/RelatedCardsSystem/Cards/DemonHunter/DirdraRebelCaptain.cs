using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class DirdraRebelCaptain: CrewmateGenerator, ICardWithRelatedCards, ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.DirdraRebelCaptain;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(Crewmates.Any(c => c?.Id == card.Id));

}
