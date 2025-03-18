using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public abstract class DormantDreadseedsGenerator
{
	protected readonly List<Card?> DormantDreadseeds = new() {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Demonhunter.GrimHarvest_CrowDreadseedToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Demonhunter.GrimHarvest_HoundDreadseedToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Demonhunter.GrimHarvest_SerpentDreadseedToken),
	};

	public List<Card?> GetRelatedCards(Player player) =>
		DormantDreadseeds;
}
