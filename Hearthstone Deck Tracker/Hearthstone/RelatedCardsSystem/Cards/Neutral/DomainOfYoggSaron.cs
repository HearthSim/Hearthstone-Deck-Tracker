using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// Mastermind Yogg-Saron ability: "Draw 5 spells."
public class DomainOfYoggSaron : ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.MastermindYoggSaron_DomainOfYoggSaronToken;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.SPELL);
}
