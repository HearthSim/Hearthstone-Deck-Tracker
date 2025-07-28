using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class WebOfDeception: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.WebOfDeception;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new()
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Rogue.WebofDeception_SkitteringSpiderlingToken),
		};
}
