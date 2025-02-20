using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

public class QualityAssurance : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warrior.QualityAssurance;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Minion" && card.GetTag(GameTag.TAUNT) > 0);
}
