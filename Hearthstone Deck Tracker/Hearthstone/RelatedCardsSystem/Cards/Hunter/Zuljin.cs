using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class Zuljin: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.Zuljin;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		player.SpellsPlayedCards
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.OrderByDescending(card => card!.Cost)
			.ToList();
}
