using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class Slaglaw: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Slagclaw;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.Collectible.Neutral.SizzlingCinder),
			Database.GetCardFromId(HearthDb.CardIds.Collectible.Neutral.SizzlingCinder)
		};
}
