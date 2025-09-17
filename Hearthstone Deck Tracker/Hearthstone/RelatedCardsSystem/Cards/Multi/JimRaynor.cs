using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Multi;

public class JimRaynor: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Invalid.JimRaynor;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentGameType, Core.Game.CurrentFormatType , opponent.OriginalClass) && GetRelatedCards(opponent).Count > 0;
	}

	public List<Card?> GetRelatedCards(Player player) =>
		player.LaunchedStarships.Select(Database.GetCardFromId)
			.ToList();
}
