﻿using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class RaDen: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.RaDen;

	public bool ShouldShowForOpponent(Player opponent) => false;
	public List<Card?> GetRelatedCards(Player player) =>
		player.CardsPlayedThisMatch
			.Where(entity => entity.Info.Created && entity.CardId != GetCardId())
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card is { Type: "Minion" })
			.OrderByDescending(card => card!.Cost)
			.ToList();
}
