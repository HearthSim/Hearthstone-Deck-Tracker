using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class HydrationStation: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.HydrationStation;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentFormat, opponent.Class) && GetRelatedCards(opponent).Count > 1;
	}

	public List<Card?> GetRelatedCards(Player player) =>
		player.DeadMinionsCards
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Distinct()
			.Where(card => card is { Mechanics: not null } && card.Mechanics.Contains("Taunt"))
			.OrderByDescending(card => card!.Cost)
			.ToList();
}
