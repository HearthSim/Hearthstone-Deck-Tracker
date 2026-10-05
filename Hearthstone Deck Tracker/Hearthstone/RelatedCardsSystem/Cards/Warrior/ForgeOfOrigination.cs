using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Summon a 3/3 Golem with Taunt. Spend 3 Armor to give it +3/+3."
public class ForgeOfOrigination : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warrior.ForgeOfOrigination;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warrior.ForgeofOrigination_SteelGolemToken),
		};
}
