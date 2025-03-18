using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class BrollBearmantle : ICardWithHighlight, ICardWithRelatedCards
{
	protected readonly List<Card?> AnimalCompanions = new() {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Hunter.MishaLegacy),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Hunter.LeokkLegacy),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Hunter.HufferLegacy),
	};
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.BrollBearmantle;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Type == "Spell");

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => AnimalCompanions;
}
