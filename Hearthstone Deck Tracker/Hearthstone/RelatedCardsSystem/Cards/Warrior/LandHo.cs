using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Draw 2 cards. Summon two 1/1 Cannoneers."
public class LandHo : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warrior.LandHo;

	public bool ShouldShowForOpponent(Player opponent) => false;

	// Two tiles on purpose: the card summons two Cannoneers.
	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warrior.Cannonmaster_CannoneerToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warrior.Cannonmaster_CannoneerToken),
		};
}
