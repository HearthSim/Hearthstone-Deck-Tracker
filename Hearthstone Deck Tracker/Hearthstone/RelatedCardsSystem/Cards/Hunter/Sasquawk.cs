using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class Sasquawk: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.Sasquawk;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentFormat, opponent.Class) && GetRelatedCards(opponent).Count > 0;
	}

	public List<Card?> GetRelatedCards(Player player) =>
		player.CardsPlayedLastTurn
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card != null)
			.OrderByDescending(card => card!.Cost)
			.ToList();
}
