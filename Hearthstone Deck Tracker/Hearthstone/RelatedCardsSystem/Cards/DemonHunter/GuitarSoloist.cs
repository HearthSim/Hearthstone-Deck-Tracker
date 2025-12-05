using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class GuitarSoloist : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.GuitarSoloist;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card.TypeEnum == CardType.SPELL,
			card.TypeEnum == CardType.MINION,
			card.TypeEnum == CardType.WEAPON
		);
}
