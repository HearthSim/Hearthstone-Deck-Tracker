using HearthDb.Enums;
using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class SketchArtist : ICardWithHighlight, ISpellSchoolTutor, ICardGenerator
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.SketchArtist;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.SHADOW);

	public int[] TutoredSpellSchools { get; } = { (int)SpellSchool.SHADOW };

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.Type == "Spell" &&
		       card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.SHADOW &&
		       card.IsCardLegal(gameMode, format);
	}

}
