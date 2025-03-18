using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class ArchdruidOfThorns: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.ArchdruidOfThorns;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		player.DeadMinionsCards
			.Where(entity => entity.Info.Turn == Core.Game.GetTurnNumber())
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card is not null && card.HasDeathrattle())
			.OrderByDescending(card => card!.Cost)
			.ToList();
}
