using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Discover two Arcane spells from the past. They cost (2) less."
public class AlterTime : FromThePastPoolCard, ICardGenerator
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.AlterTime;
	public override int EventCount() => 2;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL }
				&& (c.IsClass(playerClass) || c.IsClass("Neutral"))
				&& c.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.ARCANE)
			.Select(c => new Card(c));
	}

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
