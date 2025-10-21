using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Multi;

public class FiddlefireImp : ICardGenerator
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.FiddlefireImp;

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.Type == "Spell" &&
		       card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FIRE &&
		       (card.IsClass("Mage") || card.IsClass("Warlock")) &&
		       card.IsCardLegal(gameMode, format);
	}
}
