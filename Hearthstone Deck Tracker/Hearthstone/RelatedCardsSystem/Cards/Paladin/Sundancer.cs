using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "At the end of your turn, get a 2-Cost Blessing that gives +2/+3."
public class Sundancer : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.Sundancer;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Paladin.Sundancer_SunsBlessingToken),
		};
}
