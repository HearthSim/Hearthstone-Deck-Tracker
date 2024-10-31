using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class TyrsTears: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.TyrsTears_TyrsTearsToken;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentFormat, opponent.Class) && GetRelatedCards(opponent).Count > 1;
	}

	public List<Card?> GetRelatedCards(Player player) =>
		player.DeadMinionsCards
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Distinct()
			.Where(card => card != null && card.IsClass(player.Class))
			.OrderBy(card => card!.Cost)
			.ToList();
}
