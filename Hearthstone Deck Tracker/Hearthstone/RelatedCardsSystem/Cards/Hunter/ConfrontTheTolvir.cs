using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class ConfrontTheTolvir: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.ConfrontTheTolvir;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player)
	{
		return player.CardsPlayedThisMatch
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card is { Cost: 1 })
			.ToList();
	}
}
