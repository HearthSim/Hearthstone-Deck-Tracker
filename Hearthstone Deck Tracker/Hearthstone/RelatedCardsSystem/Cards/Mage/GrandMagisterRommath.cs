using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class GrandMagisterRommath: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.GrandMagisterRommath;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentFormat, opponent.OriginalClass) && GetRelatedCards(opponent).Count >= 2;
	}

	public List<Card?> GetRelatedCards(Player player) =>
		player.SpellsPlayedCards
			.Where(entity => entity.Info.Created)
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card is not null)
			.ToList();
}
