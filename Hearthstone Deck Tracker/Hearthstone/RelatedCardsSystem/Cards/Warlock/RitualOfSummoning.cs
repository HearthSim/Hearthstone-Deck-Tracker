using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "In 2 turns, summon two 3/5 C'thraxxi with Taunt."
public class RitualOfSummoning : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.RitualOfSummoning;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warlock.RitualofSummoning_CthraxBruteToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warlock.RitualofSummoning_CthraxBruteToken),
		};
}
