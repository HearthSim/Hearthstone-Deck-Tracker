using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "After your hero attacks, your Cannoneers FIRE!"
public class HandCannon : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warrior.HandCannon;

	public bool ShouldShowForOpponent(Player opponent) => false;

	// The weapon summons nothing; the single tile names the token it works with.
	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warrior.Cannonmaster_CannoneerToken),
		};
}
