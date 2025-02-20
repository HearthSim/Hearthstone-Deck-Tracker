namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class GuitarSoloist : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.GuitarSoloist;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(
			card.Type == "Spell",
			card.Type == "Minion",
			card.Type == "Weapon"
		);
}
