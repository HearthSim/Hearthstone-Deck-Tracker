namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class VolumeUp : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.WatercolorArtist;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell");
}
