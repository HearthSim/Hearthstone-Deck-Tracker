using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

public class PastSilvermoon: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Hunter.PastSilvermoon;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => new List<Card?>()
	{
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Hunter.PastSilvermoon_PresentSilvermoonToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Hunter.PastSilvermoon_FutureSilvermoonToken),
	};
}

public class PresentSilvermoon: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Hunter.PastSilvermoon_PresentSilvermoonToken;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => new List<Card?>()
	{
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Hunter.PastSilvermoon_FutureSilvermoonToken),
	};
}
