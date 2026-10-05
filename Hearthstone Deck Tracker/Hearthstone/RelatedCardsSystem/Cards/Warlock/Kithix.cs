using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Battlecry: If you've completed a Ritual this game, choose a Sinister Ritual to begin."
public class Kithix : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.Kithix;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warlock.Kithix_RitualOfFamineToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warlock.Kithix_RitualOfSubjugationToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warlock.Kithix_RitualOfConquestToken),
		};
}
