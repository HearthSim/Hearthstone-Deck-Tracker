using System;
using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class Reminisce: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Priest.Reminisce;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player)
	{
		var opponent = Core.Game.Player.Id == player.Id ? Core.Game.Opponent : Core.Game.Player;

		return opponent.CardsPlayedThisMatch
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card != null)
			.Skip(Math.Max(0, opponent.CardsPlayedThisMatch.Count - 2))
			.ToList();
	}

}
