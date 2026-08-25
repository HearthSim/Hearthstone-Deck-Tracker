using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Resummon all friendly minions that were slimed."
// Token created for both players by Slime 'em! ("Destroy all minions. Each player gets a 3-Cost
// spell that resummons theirs."). PowerHandler snapshots each side's board into Player.SlimedMinions
// when Slime 'em! resolves, so this shows exactly what the holder's copy will bring back.
public class Ectoplasm : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Priest.Slimeem_EctoplasmToken;

	public bool ShouldShowForOpponent(Player opponent) => false;

	// Duplicates are kept: two copies of a minion on board are two resummons.
	public List<Card?> GetRelatedCards(Player player) =>
		player.SlimedMinions
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.OrderByDescending(card => card?.Cost)
			.ToList();
}
