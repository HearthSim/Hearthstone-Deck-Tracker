using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class UmbralGeist : ICardGenerator
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.UmbralGeist;

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.Type == "Spell" &&
		       card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.SHADOW &&
		       card.IsCardLegal(gameMode, format);
	}
}
