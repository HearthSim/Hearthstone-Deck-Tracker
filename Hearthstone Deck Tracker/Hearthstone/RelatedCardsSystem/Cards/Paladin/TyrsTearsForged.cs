using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class TyrsTearsForged: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Paladin.TyrsTears;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		player.DeadMinionsCardIds
			.Distinct()
			.Select(Database.GetCardFromId)
			.Where(card => card != null && card.IsClass(player.Class))
			.OrderBy(card => card!.Cost)
			.ToList();
}
