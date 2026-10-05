using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Summon two 0/3 Guards with Taunt. Assemble a minion."
public class DarkestDepths : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.DarkestDepths;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warlock.DarkestDepths_DepthguardToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Warlock.DarkestDepths_DepthguardToken),
		};
}
