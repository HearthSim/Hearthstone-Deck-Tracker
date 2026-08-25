using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Battlecry: Resurrect your minions that were Reborn this game. They attack random enemy minions."
public class RaithVanGeist : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.RaithVanGeist;

	public bool ShouldShowForOpponent(Player opponent) => false;

	// HAS_BEEN_REBORN is stamped on the *new* entity the Reborn resummon creates, not on the minion that
	// died, so the pool cannot be read off DeadMinionsCards: it would only list a minion once its reborn
	// copy had also died. Scan every entity instead - one HAS_BEEN_REBORN stamp per Reborn that resolved.
	public List<Card?> GetRelatedCards(Player player) =>
		player.PlayerEntities
			.Where(entity => entity.IsMinion && entity.HasTag(GameTag.HAS_BEEN_REBORN))
			.Select(e => CardUtils.GetProcessedCardFromEntity(e, player))
			.Where(card => card != null)
			.OrderByDescending(card => card!.Cost)
			.ToList();
}
