using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class PrimordialProtector : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.PrimordialProtector;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck)
	{
		var spells = deck.Where(c => c.Type == "Spell").ToArray();
		if (spells.Length == 0)
		{
			return HighlightColor.None;
		}
		var highestCost = spells.Max(c => c.Cost);
		return HighlightColorHelper.GetHighlightColor(
			card.TypeEnum == CardType.SPELL && card.Cost == highestCost
		);
	}
}
