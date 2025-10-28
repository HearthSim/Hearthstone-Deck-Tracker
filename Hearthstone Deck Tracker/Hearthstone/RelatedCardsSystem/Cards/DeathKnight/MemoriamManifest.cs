using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class MemoriamManifest: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.MemoriamManifest;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player)
	{
		var undeadsThatDied = player.DeadMinionsCards
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card != null && card.IsUndead()).ToList();

		if(!undeadsThatDied.Any())
		{
			return new List<Card?>();
		}
		var highestCost = undeadsThatDied.Max(c => c?.Cost);
		return undeadsThatDied.Where(card => card is not null && card.Cost == highestCost).ToList();
	}
}
