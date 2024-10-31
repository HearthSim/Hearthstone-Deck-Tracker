using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class StranglethornHeart: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.StranglethornHeart;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentFormat, opponent.Class) && GetRelatedCards(opponent).Count > 1;
	}

	public List<Card?> GetRelatedCards(Player player) =>
		player.DeadMinionsCards
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card != null && card.IsBeast() && card.Cost > 4)
			.OrderByDescending(card => card!.Cost)
			.ToList();
}
