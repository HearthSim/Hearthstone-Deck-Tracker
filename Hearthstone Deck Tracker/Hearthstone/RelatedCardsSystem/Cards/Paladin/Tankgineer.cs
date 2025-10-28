using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class Tankgineer: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.Tankgineer;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => new List<Card?>()
		{ Database.GetCardFromId(HearthDb.CardIds.Collectible.Neutral.ForceTankMax) };
}
