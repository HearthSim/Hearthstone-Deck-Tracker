using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class ChattyMacaw: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.ChattyMacaw;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player)
	{
		var lastCard = player.SpellsPlayedInOpponentCharacters
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.LastOrDefault();

		return lastCard != null ? new List<Card?> { lastCard } : new List<Card?>();
	}
}
