using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

public class InventorBoom: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warrior.InventorBoom;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentFormat, opponent.Class) && GetRelatedCards(opponent).Count > 0;
	}

	public List<Card?> GetRelatedCards(Player player) =>
		player.DeadMinionsCards
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Distinct()
			.Where(card => card != null && card.IsMech() && card.Cost > 4)
			.OrderBy(card => card!.Cost)
			.ToList();
}
