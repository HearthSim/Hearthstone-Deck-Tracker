using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class TheGalacticProjectionOrb: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.TheGalacticProjectionOrb;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentFormat, opponent.Class) && GetRelatedCards(opponent).Count > 1;
	}

	public List<Card?> GetRelatedCards(Player player) =>
		player.SpellsPlayedCards
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Distinct()
			.Where(card => card != null)
			.OrderBy(card => card!.Cost)
			.ToList();
}
