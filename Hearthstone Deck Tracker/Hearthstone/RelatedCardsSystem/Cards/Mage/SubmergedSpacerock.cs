using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Deathrattle: Add two Arcane Mage spells to your hand. They are Temporary."
// Temporary is a post-pick modifier; the pool is Arcane Mage spells.
public class SubmergedSpacerock : DiscoverPoolCard, ICardGenerator
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.SubmergedSpacerock;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL }
				&& c.IsClass("Mage")
				&& c.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.ARCANE)
			.Select(c => new Card(c));
	}

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.TypeEnum == CardType.SPELL &&
		       card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.ARCANE &&
		       card.IsCardLegal(gameMode, format);
	}

	public bool IsInGeneratorPool(MultiIdCard card, GameType gameMode, FormatType format)
	{
		return card.Ids.Any(c => IsInGeneratorPool(new Card(c), gameMode, format));
	}
}
