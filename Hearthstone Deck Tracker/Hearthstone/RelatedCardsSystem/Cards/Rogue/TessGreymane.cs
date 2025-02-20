using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class TessGreymane: ICardWithRelatedCards
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Rogue.TessGreymane;

	public bool ShouldShowForOpponent(Player opponent)
	{
		return false;
	}

	public List<Card?> GetRelatedCards(Player player) =>
		player.CardsPlayedThisMatch
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card != null && !card.IsClass(player.CurrentClass) && !card.IsNeutral)
			.OrderBy(card => card!.Cost)
			.ToList();
}

public class TessGreymaneCore: TessGreymane
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.TessGreymaneCore;
}
