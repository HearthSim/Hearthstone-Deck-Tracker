using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Taunt. Battlecry: Summon a {0}/{1} Beast with Rush. (It has the opposite stats of this minion!)"
public class CuriousGrimlynx : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.TwistedGrimlynx_CuriousGrimlynxToken;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Druid.TwistedGrimlynx),
		};
}
