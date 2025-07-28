using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class VioletSpellwing: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.VioletSpellwing;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new()
		{
			Database.GetCardFromId(HearthDb.CardIds.Collectible.Mage.ArcaneMissilesLegacy),
		};
}
