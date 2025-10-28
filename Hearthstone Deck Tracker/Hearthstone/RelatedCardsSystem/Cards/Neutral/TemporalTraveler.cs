using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class TemporalTraveler: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.TemporalTraveler;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => new List<Card?>() 
		{ Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.TemporalTraveler_TemporalShadowToken) };
}
