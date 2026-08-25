using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Your Cannoneers fire an additional shot. Battlecry: Summon two 1/1 Cannoneers."
public class CaptainCrowley : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warrior.CaptainCrowley;

	public bool ShouldShowForOpponent(Player opponent) => false;

	// Two tiles on purpose: the Battlecry summons two Cannoneers.
	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warrior.Cannonmaster_CannoneerToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warrior.Cannonmaster_CannoneerToken),
		};
}
