using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

public class TwilightMender : ICardGenerator
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.TwilightMender;

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.TypeEnum == CardType.SPELL &&
		       (card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.HOLY || card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.SHADOW ) &&
		       card.IsCardLegal(gameMode, format);
	}

	public bool IsInGeneratorPool(MultiIdCard card, GameType gameMode, FormatType format)
	{
		return card.Ids.Any(c => IsInGeneratorPool(new Card(c), gameMode, format));
	}
}
