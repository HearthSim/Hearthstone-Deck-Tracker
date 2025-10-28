using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class PastConflux: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.PastConflux;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => new List<Card?>()
	{
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Priest.PastConflux_PresentConfluxToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Priest.PastConflux_FutureConfluxToken),
	};
}

public class PresentConflux: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Priest.PastConflux_PresentConfluxToken;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => new List<Card?>()
	{
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Priest.PastConflux_FutureConfluxToken),
	};
}
