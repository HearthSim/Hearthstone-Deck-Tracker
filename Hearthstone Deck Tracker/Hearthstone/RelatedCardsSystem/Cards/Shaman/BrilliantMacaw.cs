using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class BrilliantMacaw: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.BrilliantMacaw;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player)
	{
		var lastBattlecry = player.CardsPlayedThisMatch
										.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
										.LastOrDefault(card => card is { Mechanics: not null } && card.Mechanics.Contains("Battlecry"));
		return lastBattlecry != null ? new List<Card?> { lastBattlecry } : new List<Card?>();
	}
}
