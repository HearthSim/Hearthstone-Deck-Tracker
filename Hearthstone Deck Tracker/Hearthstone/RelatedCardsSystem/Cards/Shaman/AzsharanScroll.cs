using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Discover a Fire, Frost or Nature spell. Put a 'Sunken Scroll' on the bottom of your deck."
public class AzsharanScroll : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.AzsharanScroll;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL }
				&& (c.IsClass(playerClass) || c.IsClass("Neutral"))
				&& (c.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FIRE
					|| c.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FROST
					|| c.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.NATURE))
			.Select(c => new Card(c));
	}
}

// "Add a Fire, Frost, and Nature spell from your class to your hand." (Sunken Scroll, shuffled in by Azsharan Scroll)
public class SunkenScrollToken : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Shaman.AzsharanScroll_SunkenScrollToken;
	public override int Picks() => 1;
	public override int EventCount() => 3;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		var fire = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL }
				&& c.IsClass(playerClass) && c.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FIRE)
			.Select(c => new Card(c));
		var frost = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL }
				&& c.IsClass(playerClass) && c.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.FROST)
			.Select(c => new Card(c));
		var nature = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL }
				&& c.IsClass(playerClass) && c.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.NATURE)
			.Select(c => new Card(c));
		return fire.Concat(frost).Concat(nature);
	}
}
