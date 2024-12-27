using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class PetParrot: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.PetParrot;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player)
	{
		var lastCost1 = player.CardsPlayedThisMatch
									.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
									.LastOrDefault(card => card is { Cost: 1 });
		return lastCost1 != null ? new List<Card?> { lastCost1 } : new List<Card?>();
	}
}
