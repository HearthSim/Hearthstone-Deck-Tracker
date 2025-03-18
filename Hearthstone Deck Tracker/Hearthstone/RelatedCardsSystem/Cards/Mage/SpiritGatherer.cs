using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class SpiritGatherer: ICardWithRelatedCards
{
	protected readonly List<Card?> Wisp = new() {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Mage.WispTokenEMERALD_DREAM),
	};

	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.SpiritGatherer;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => Wisp;
}
