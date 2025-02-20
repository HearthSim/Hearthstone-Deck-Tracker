namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class Banjosaur : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.Banjosaur;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.IsBeast());
}
