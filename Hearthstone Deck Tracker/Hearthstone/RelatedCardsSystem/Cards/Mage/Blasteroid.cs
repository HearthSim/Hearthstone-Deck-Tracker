using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class Blasteroid : ICardGenerator
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.Blasteroid;

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.Type == "Spell" &&
		       card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FIRE &&
		       card.IsCardLegal(gameMode, format);
	}
}
