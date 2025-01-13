using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class FateSplitter: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.FateSplitter;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player)
	{

		var opponent = Core.Game.Player.Id == player.Id ? Core.Game.Opponent : Core.Game.Player;

		var lastCard = opponent.CardsPlayedThisMatch
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, opponent))
			.LastOrDefault(card => card != null);

		return lastCard != null ? new List<Card?> { lastCard } : new List<Card?>();
	}
}
