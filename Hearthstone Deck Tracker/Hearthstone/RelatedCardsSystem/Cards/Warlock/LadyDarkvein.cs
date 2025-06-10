using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class LadyDarkvein: ICardWithRelatedCards
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Warlock.LadyDarkvein;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player)
	{
		var lastShadowSpell = player.SpellsPlayedCards
			.Select(entity => CardUtils.GetProcessedCardFromEntity(entity, player))
			.LastOrDefault(card => card != null && card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.SHADOW);
		return lastShadowSpell != null ? new List<Card?> { lastShadowSpell } : new List<Card?>();
	}
}

public class LadyDarkveinCorePlaceholder : LadyDarkvein
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.LadyDarkveinCorePlaceholder;
}
