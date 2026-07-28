using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Multi;

// "Battlecry: Add a random Fire Mage and Fire Warlock spell to your hand."
public class FiddlefireImp : DiscoverPoolCard, ICardGenerator
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.FiddlefireImp;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL }
				&& (c.IsClass("Mage") || c.IsClass("Warlock"))
				&& c.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FIRE)
			.Select(c => new Card(c));
	}

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.TypeEnum == CardType.SPELL &&
		       card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FIRE &&
		       (card.IsClass("Mage") || card.IsClass("Warlock")) &&
		       card.IsCardLegal(gameMode, format);
	}

	public bool IsInGeneratorPool(MultiIdCard card, GameType gameMode, FormatType format)
	{
		return card.Ids.Any(c => IsInGeneratorPool(new Card(c), gameMode, format));
	}
}
