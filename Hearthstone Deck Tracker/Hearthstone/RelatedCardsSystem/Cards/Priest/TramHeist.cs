using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class TramHeist: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.TramHeist;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player)
	{

		var opponent = Core.Game.Player.Id == player.Id ? Core.Game.Opponent : Core.Game.Player;

		return opponent.CardsPlayedLastTurn
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, opponent))
			.Where(card => card != null)
			.OrderByDescending(card => card!.Cost)
			.ToList();
	}
}
