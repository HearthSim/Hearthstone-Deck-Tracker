using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class TessGreymane: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.TessGreymane;

	public bool ShouldShowForOpponent(Player opponent)
	{
		return false;
	}

	public List<Card?> GetRelatedCards(Player player) =>
		player.CardsPlayedThisMatch
			.Select(Database.GetCardFromId)
			.Where(card => card != null && !card.IsClass(player.Class) && !card.IsNeutral)
			.OrderBy(card => card!.Cost)
			.ToList();
}
