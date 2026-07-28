using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Deathrattle: Get a random Holy and Shadow spell."
// Two events from different sub-pools (one Holy, one Shadow) approximated as two draws
// from the union pool, like FiddlefireImp.
public class TwilightMender : DiscoverPoolCard, ICardGenerator
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.TwilightMender;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL }
				&& (c.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.HOLY
					|| c.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.SHADOW))
			.Select(c => new Card(c));
	}

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
