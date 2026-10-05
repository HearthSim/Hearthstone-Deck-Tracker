using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Draw two cards. For each that isn't a minion, summon a 3/6 Water Elemental."
public class IcebeltMagi : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.IcebeltMagi;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.Collectible.Mage.WaterElementalLegacy),
		};
}
