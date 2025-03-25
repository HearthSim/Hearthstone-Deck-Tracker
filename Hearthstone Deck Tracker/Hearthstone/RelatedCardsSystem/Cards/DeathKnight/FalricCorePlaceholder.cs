using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class FalricCorePlaceholder : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.FalricCore;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag((GameTag)4058) > 0);
}
