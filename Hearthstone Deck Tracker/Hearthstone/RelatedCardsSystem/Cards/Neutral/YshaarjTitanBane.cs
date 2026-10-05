using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Old God. Battlecry: If you've spent 30 Mana on minions this game, gain Immune for 1 turn."
public class YshaarjTitanBane : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.YshaarjTitanBane;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.YShaarjTitanBane_GazeOfYshaarjToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.YShaarjTitanBane_SpawnOfYshaarjToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.YShaarjTitanBane_MightOfYshaarjToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.YShaarjTitanBane_DomainOfYshaarjToken1),
		};
}
