using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class LadyLiadrin: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.LadyLiadrin;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		player.SpellsPlayedInFriendlyCharacters
			.Select(entity => CardUtils.GetProcessedCardFromCardId(entity.CardId, player))
			.ToList();
}
