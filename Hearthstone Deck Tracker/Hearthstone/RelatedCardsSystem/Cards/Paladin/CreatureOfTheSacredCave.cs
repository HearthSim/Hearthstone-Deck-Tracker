using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class CreatureOfTheSacredCave: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.CreatureOfTheSacredCave;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		player.CardsPlayedThisTurn
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card?.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.HOLY)
			.ToList();
}
