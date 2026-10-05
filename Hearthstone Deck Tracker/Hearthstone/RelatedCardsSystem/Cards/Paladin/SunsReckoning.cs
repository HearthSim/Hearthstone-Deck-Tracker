using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Deal 1 damage to all enemy minions. Shuffle 'Final Reckoning' into your deck."
public class SunsReckoning : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.SunsReckoning;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Paladin.SunsReckoning_FinalReckoningToken),
		};
}
