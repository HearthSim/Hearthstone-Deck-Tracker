using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class LadyDeathwhisper : ICardGenerator
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.LadyDeathwhisper;

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.Type == "Spell" &&
		       card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FROST &&
		       card.IsCardLegal(gameMode, format);
	}
}
