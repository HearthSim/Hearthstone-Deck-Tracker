using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Taunt. Battlecry: Summon a 0, 1, and 2-Attack minion from your deck. Gain their stats."
public class Zerontu : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.Zerontu;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			card is { TypeEnum: CardType.MINION, Attack: 0 },
			card is { TypeEnum: CardType.MINION, Attack: 1 },
			card is { TypeEnum: CardType.MINION, Attack: 2 }
		);
}
