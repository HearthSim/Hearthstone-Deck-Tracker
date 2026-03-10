using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class CaliaMenethil: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.CaliaMenethilCorePlaceholder;

	public bool ShouldShowForOpponent(Player opponent) => false;
	public List<Card?> GetRelatedCards(Player player)
	{
		var highestCostMinions = new List<Card?>();

		var playerMinions =  player.DeadMinionsCards.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card is not null).ToList();

		if(playerMinions.Any())
		{
			var highestCost = playerMinions.Max(c => c?.Cost);
			var minionsWithHighestCost = playerMinions.Where(c => (c?.Cost ?? 0) == highestCost);
			highestCostMinions.AddRange(minionsWithHighestCost);
		}

		return highestCostMinions;
	}
}
