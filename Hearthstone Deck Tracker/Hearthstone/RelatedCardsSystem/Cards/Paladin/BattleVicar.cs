using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Battlecry: Discover a Holy spell."
public class BattleVicar : DiscoverPoolCard, ICardGenerator
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.BattleVicar;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL }
				&& (c.IsClass(playerClass) || c.IsClass("Neutral"))
				&& c.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.HOLY)
			.Select(c => new Card(c));
	}

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.TypeEnum == CardType.SPELL &&
		       card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.HOLY &&
		       card.IsCardLegal(gameMode, format);
	}

	public bool IsInGeneratorPool(MultiIdCard card, GameType gameMode, FormatType format)
	{
		return card.Ids.Any(c => IsInGeneratorPool(new Card(c), gameMode, format));
	}
}

public class BattleVicarCore : BattleVicar
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.BattleVicarCore;
}
