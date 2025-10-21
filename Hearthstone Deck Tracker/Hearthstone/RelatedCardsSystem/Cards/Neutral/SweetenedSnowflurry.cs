using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class SweetenedSnowflurry : ICardGenerator
{
	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Neutral.SweetenedSnowflurry;

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.Type == "Spell" &&
		       card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FROST &&
		       card.IsCardLegal(gameMode, format);
	}
}

public class SweetenedSnowfluryMini : SweetenedSnowflurry
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.SweetenedSnowflurry_SweetenedSnowflurryToken;
}
