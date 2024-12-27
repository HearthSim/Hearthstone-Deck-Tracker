using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class Tyr: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.Tyr;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		player.DeadMinionsCards
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Distinct()
			.Where(card => card != null && card.IsClass(player.CurrentClass) && card.Attack is > 1 and < 5)
			.OrderBy(card => card!.Cost)
			.ToList();
}
