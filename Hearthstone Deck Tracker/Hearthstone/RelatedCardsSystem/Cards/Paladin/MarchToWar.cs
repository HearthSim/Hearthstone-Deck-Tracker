using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "All numbers on this card equal your remaining Mana. Summon a 1/1 Soldier with Divine Shield."
public class MarchToWar : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.MarchToWar;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Paladin.MarchtoWar_VanirSoldierToken),
		};
}
