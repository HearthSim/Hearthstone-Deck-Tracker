using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

public class RavenousFlock: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.RavenousFlock;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Druid.SkyscreamerEggs_SkyscreamerHatchlingToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Druid.SkyscreamerEggs_SkyscreamerHatchlingToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Druid.SkyscreamerEggs_SkyscreamerHatchlingToken),
		};
}
