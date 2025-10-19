using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class GreySageParrot: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.GreySageParrot;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player)
	{
		var lastSpell = player.CardsPlayedThisMatch
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.LastOrDefault(card => card is { Type: "Spell", Cost: >= 6 });
		return lastSpell != null ? new List<Card?> { lastSpell } : new List<Card?>();
	}
}
