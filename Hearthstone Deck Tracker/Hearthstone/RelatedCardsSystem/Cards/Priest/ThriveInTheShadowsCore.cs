namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class ThriveInTheShadowsCore : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.ThriveInTheShadowsCore;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell");
}
