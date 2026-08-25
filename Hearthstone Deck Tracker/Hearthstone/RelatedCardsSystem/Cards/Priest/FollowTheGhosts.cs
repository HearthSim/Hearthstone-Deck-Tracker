using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Summon a 2/1 Ghost with Reborn. Give a playable card in your hand this effect for a turn."
public class FollowTheGhosts : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.FollowTheGhosts;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Priest.FollowtheGhosts_SpookyGhostToken),
		};
}
