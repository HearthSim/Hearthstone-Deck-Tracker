using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class CardboardGolem : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.CardboardGolem;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.PALADIN_AURA) > 0);
}
