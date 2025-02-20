namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class CactusCutter : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.CactusCutter;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell");
}
