using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class VelenLeaderOfTheExiled: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.VelenLeaderOfTheExiled;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.IsCardFromFormat(card, Core.Game.CurrentFormat) && GetRelatedCards(opponent).Count > 2;
	}

	public List<Card?> GetRelatedCards(Player player) =>
		player.CardsPlayedThisMatch
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card is { Mechanics: not null } && card.isDraenei() && card.Id != GetCardId() &&
			               (card.Mechanics.Contains("Battlecry") ||  card.Mechanics.Contains("Deathrattle")))
			.ToList();
}
