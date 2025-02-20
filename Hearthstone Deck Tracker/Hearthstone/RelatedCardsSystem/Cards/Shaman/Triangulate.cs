namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class Triangulate : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Triangulate;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell" && card.Id != GetCardId());
}
