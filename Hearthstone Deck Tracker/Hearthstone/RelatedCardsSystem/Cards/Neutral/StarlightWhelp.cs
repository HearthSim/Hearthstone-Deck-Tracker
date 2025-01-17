using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class StarlightWhelp: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.StarlightWhelp;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		player.StartingHand
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.ToList();
}
