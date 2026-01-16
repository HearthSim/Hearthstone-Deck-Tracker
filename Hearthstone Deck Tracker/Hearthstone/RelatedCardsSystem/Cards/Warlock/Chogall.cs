using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class Chogall: ICardWithRelatedCards
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Warlock.ChogallOG;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentGameType, Core.Game.CurrentFormatType , opponent.OriginalClass) && GetRelatedCards(opponent).Count > 2;
	}

	public List<Card?> GetRelatedCards(Player player) =>
		player.EntitiesDiscardedFromHand
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.ToList();
}

public class ChogallWONDERS : Chogall
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.ChogallWONDERS;
}
