using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

public class UnleashTheCrocolisks: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warrior.UnleashTheCrocolisks;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warrior.UnleashtheCrocolisks_ColiseumCrocoliskToken),
		};
}
