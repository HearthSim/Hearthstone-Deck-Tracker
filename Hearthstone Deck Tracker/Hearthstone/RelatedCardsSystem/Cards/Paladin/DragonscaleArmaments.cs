using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class DragonscaleArmaments : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.DragonscaleArmaments;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card is { Type: "Spell", IsCreated: true },
			card is { Type: "Spell", IsCreated: false }
		);
}
