using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class KragwaTheFrog: ICardWithRelatedCards
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Shaman.KragwaTheFrog;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		player.CardsPlayedLastTurn
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card is { Type: "Spell" })
			.OrderByDescending(card => card!.Cost)
			.ToList();
}

public class KragwaTheFrogCore: KragwaTheFrog
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.KragwaTheFrogCorePlaceholder;
}
