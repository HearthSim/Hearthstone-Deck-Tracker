using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Get two 2/1 Plants with 'Deathrattle: Summon a 5/5 Bug with Taunt.' They cost (2)."
public class Deathblossom : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Druid.Deathblossom;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Druid.Deathblossom_CarnivorousPlantToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Druid.Deathblossom_CarnivorousPlantToken),
		};
}
