using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class PlumeOfVulcanos : ICardGenerator
{
	public virtual string GetCardId() => HearthDb.CardIds.NonCollectible.Mage.Vulcanos_PlumeOfVulcanosToken1;

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.TypeEnum == CardType.SPELL &&
		       card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FIRE &&
		       card.IsCardLegal(gameMode, format);
	}

	public bool IsInGeneratorPool(MultiIdCard card, GameType gameMode, FormatType format)
	{
		return card.Ids.Any(c => IsInGeneratorPool(new Card(c), gameMode, format));
	}
}

public class PlumeOfVulcanos2 : PlumeOfVulcanos
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Mage.Vulcanos_PlumeOfVulcanosToken2;
}
