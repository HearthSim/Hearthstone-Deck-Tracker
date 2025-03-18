using System.Linq;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class InterstellarResearcher : ICardWithHighlight
{
	private readonly string[] _libramCardIds =
	{
		HearthDb.CardIds.Collectible.Paladin.LibramOfWisdom,
		HearthDb.CardIds.Collectible.Paladin.LibramOfClarity,
		HearthDb.CardIds.Collectible.Paladin.LibramOfDivinity,
		HearthDb.CardIds.Collectible.Paladin.LibramOfJustice,
		HearthDb.CardIds.Collectible.Paladin.LibramOfFaith,
		HearthDb.CardIds.Collectible.Paladin.LibramOfJudgment,
		HearthDb.CardIds.Collectible.Paladin.LibramOfHope,
	};
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.InterstellarResearcher;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(
			_libramCardIds.Contains(card.Id));
}
