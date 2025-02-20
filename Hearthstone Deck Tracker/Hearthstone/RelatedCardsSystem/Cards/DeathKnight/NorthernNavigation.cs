using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class NorthernNavigation : ICardWithHighlight
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.NorthernNavigation;

	public HighlightColor ShouldHighlight(Card card) =>
		HighlightColorHelper.GetHighlightColor(
			card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FROST,
			card.Type == "Spell");
}
