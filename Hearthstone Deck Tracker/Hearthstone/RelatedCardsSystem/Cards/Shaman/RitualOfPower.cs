using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

public class RitualOfPower: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Shaman.RitualOfPower;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Shaman.RitualofPower_BreezlingToken),
		};
}
