using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class Rewind: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.Rewind;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		player.SpellsPlayedCards
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Distinct()
			.Where(card => card != null && card.Id != HearthDb.CardIds.Collectible.Mage.Rewind)
			.OrderByDescending(card => card!.Cost)
			.ToList();
}
