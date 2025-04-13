using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class TwistedWebweaver: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.TwistedWebweaver;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player)
	{

		return player.CardsPlayedThisMatch
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card is { Type: "Minion" })
			.Distinct()
			.OrderBy(card => card!.Cost)
			.ToList();
	}
}
