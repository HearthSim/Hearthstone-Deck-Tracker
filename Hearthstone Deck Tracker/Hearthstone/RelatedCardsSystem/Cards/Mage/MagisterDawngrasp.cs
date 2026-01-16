using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class MagisterDawngrasp: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.MagisterDawngrasp;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentGameType, Core.Game.CurrentFormatType , opponent.OriginalClass) && GetRelatedCards(opponent).Count >= 2;
	}

	public List<Card?> GetRelatedCards(Player player) =>
		player.SpellsPlayedCards
			.Where(entity => entity.HasTag(GameTag.SPELL_SCHOOL))
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card is not null)
			.Distinct()
			.OrderBy(c => c?.GetTag(GameTag.SPELL_SCHOOL))
			.ThenBy(c => c?.Cost)
			.ToList();
}
