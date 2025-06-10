using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class JaceDarkweaver: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.JaceDarkweaver;

	public bool ShouldShowForOpponent(Player opponent)
	{
		var card = Database.GetCardFromId(GetCardId());
		return CardUtils.MayCardBeRelevant(card, Core.Game.CurrentFormat, opponent.OriginalClass) && GetRelatedCards(opponent).Count > 3;
	}
	public List<Card?> GetRelatedCards(Player player) =>
		player.SpellsPlayedCards
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.Where(card => card != null && card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FEL)
			.ToList();
}
