namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class Saruun : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Mage.Saruun;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.IsElemental());
}
