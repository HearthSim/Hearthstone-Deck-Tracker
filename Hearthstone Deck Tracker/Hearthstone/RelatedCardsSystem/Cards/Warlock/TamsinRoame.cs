using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class TamsinRoame : ICardGenerator
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Warlock.TamsinRoame;

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.Type == "Spell" &&
		       card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.SHADOW &&
		       card.IsCardLegal(gameMode, format);
	}

	public bool IsInGeneratorPool(MultiIdCard card, GameType gameMode, FormatType format)
	{
		return card.Ids.Any(c => IsInGeneratorPool(new Card(c), gameMode, format));
	}
}
