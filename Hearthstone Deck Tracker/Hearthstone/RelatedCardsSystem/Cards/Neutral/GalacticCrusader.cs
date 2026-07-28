using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Taunt Deathrattle: Get two random Holy spells. They cost (3) less."
public class GalacticCrusader : HolySpellPool, ICardGenerator
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.GalacticCrusader;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;

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
