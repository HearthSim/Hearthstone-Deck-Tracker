using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class StoryOfCarnassa : ICardWithRelatedCards
{
	private readonly List<Card?> _token = new() {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Hunter.TheMarshQueen_CarnassasBroodToken)
	};
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.StoryOfCarnassa;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => _token;
}
