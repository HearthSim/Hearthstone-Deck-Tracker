using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class MirrorDimension: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.MirrorDimension;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => new List<Card?>()
	{
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Mage.MirrorDimension_MirroredMageToken),
	};
}
