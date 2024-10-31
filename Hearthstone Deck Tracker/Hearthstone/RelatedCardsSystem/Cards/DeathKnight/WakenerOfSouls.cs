using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class WakenerOfSouls: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.WakenerOfSouls;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		player.DeadMinionsCards
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Distinct()
			.Where(card => card is { Mechanics: not null }
			               && card.Id != HearthDb.CardIds.Collectible.Deathknight.WakenerOfSouls
			               && card.Mechanics.Contains("Deathrattle"))
			.OrderByDescending(card => card!.Cost)
			.ToList();
}
