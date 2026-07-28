using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Miniaturize Battlecry: Get 2 random Temporary Frost spells."
// Temporary is a post-pick modifier; the pool is the plain Frost spell pool from ColdSnap.
public class SweetenedSnowflurry : FrostSpellPool, ICardGenerator
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.SweetenedSnowflurry;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;

	public bool IsInGeneratorPool(Card card, GameType gameMode, FormatType format)
	{
		return card.TypeEnum == CardType.SPELL &&
		       card.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FROST &&
		       card.IsCardLegal(gameMode, format);
	}

	public bool IsInGeneratorPool(MultiIdCard card, GameType gameMode, FormatType format)
	{
		return card.Ids.Any(c => IsInGeneratorPool(new Card(c), gameMode, format));
	}
}

public class SweetenedSnowfluryMini : SweetenedSnowflurry
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.SweetenedSnowflurry_SweetenedSnowflurryToken;
}
