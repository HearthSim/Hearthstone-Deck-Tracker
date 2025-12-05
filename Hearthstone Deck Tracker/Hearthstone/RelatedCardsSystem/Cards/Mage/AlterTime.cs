using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class AlterTime : ICardGenerator
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.AlterTime;

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.TypeEnum == CardType.SPELL && card.IsClass("Mage") &&
		       card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.ARCANE &&
		       (Helper.WildOnlySets.Contains(card.Set) ||
		        Helper.ClassicOnlySets.Contains(card.Set));
	}

	public bool IsInGeneratorPool(MultiIdCard card, GameType gameMode, FormatType format)
	{
		return card.Ids.All(c => IsInGeneratorPool(new Card(c), gameMode, format));
	}
}
