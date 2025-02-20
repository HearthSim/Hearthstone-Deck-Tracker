namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class WeaponsAttendant : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.WeaponsAttendant;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Weapon", card.IsPirate());
}
