using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class Mixtape: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.Mixtape;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player)
	{

		var opponent = Core.Game.Player.Id == player.Id ? Core.Game.Opponent : Core.Game.Player;

		return opponent.CardsPlayedThisMatch
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, opponent))
			.Where(card => card != null)
			.Distinct()
			.ToList();
	}
}
