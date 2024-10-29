using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class TessGreymaneCore: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.TessGreymaneCore;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentFormat, opponent.Class) && GetRelatedCards(opponent).Count > 2;
	}

	public List<Card?> GetRelatedCards(Player player) =>
		player.CardsPlayedThisMatch
			.Select(entity => Database.GetCardFromId(entity.CardId))
			.Where(card => card != null && !card.IsClass(player.Class) && !card.IsNeutral)
			.OrderBy(card => card!.Cost)
			.ToList();
}
