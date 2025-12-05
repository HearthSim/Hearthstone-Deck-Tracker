using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;
using HearthDb.Enums;

public class BrollBearmantle : ICardWithHighlight, ICardWithRelatedCards
{
	protected readonly List<Card?> AnimalCompanions = new() {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Hunter.MishaLegacy),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Hunter.LeokkLegacy),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Hunter.HufferLegacy),
	};
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.BrollBearmantle;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.TypeEnum == CardType.SPELL);

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => AnimalCompanions;
}
