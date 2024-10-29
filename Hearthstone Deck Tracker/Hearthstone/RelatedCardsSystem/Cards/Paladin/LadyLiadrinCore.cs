using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class LadyLiadrinCore: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.LadyLiadrinCore;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentFormat, opponent.Class) && GetRelatedCards(opponent).Count > 2;
	}

	public List<Card?> GetRelatedCards(Player player) =>
		player.SpellsPlayedInFriendlyCharacters
			.Select(entity => Database.GetCardFromId(entity.CardId))
			.ToList();
}
