using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class TheAzeriteRat: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Deathknight.KoboldMiner_TheAzeriteRatToken;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player)
	{
		if(!player.DeadMinionsCards.Any())
		{
			return new List<Card?>();
		}
		var highestCost = player.DeadMinionsCards.Max(c => c.Cost);
		return player.DeadMinionsCards
				.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
				.Where(card => card != null && card.Cost == highestCost)
				.ToList();
	}
}
