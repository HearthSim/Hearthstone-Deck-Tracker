using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "After your hero attacks, summon two 2/2 Elementals with Rush."
public class Eyemaul : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Eyemaul;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Shaman.Eyemaul_TremoringElementalToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Shaman.Eyemaul_TremoringElementalToken),
		};
}
