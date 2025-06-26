using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class SkyscreamerEggs: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.SkyscreamerEggs;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Druid.SkyscreamerEggs_SkyscreamerHatchlingToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Druid.SkyscreamerEggs_SkyscreamerHatchlingToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Druid.SkyscreamerEggs_SkyscreamerHatchlingToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Druid.SkyscreamerEggs_SkyscreamerHatchlingToken),
		};
}
