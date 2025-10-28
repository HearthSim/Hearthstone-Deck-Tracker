using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class PastGnomeregan: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.PastGnomeregan;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => new List<Card?>()
	{
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Paladin.PastGnomeregan_PresentGnomereganToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Paladin.PastGnomeregan_FutureGnomereganToken),
	};
}

public class PresentGnomeregan: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Paladin.PastGnomeregan_PresentGnomereganToken;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => new List<Card?>()
	{
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Paladin.PastGnomeregan_FutureGnomereganToken),
	};
}
