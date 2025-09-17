using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class ReturnPolicy: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.ReturnPolicy;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentGameType, Core.Game.CurrentFormatType, opponent.OriginalClass)
		       && GetRelatedCards(opponent).Count > 1;
	}
	public List<Card?> GetRelatedCards(Player player) =>
		player.CardsPlayedThisMatch
			.Select(entity => Database.GetCardFromId(entity.CardId).HandleZilliax3000(player))
			.Distinct()
			.Where(card => card is { Mechanics: not null } && card.Mechanics.Contains("Deathrattle"))
			.OrderByDescending(card => card!.Cost)
			.ToList();
}
