using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "After a friendly Undead dies, deal 2 damage to the enemy hero and get a random Shadow Priest spell."
public class SoulburnerVaria : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.SoulburnerVaria;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL }
				&& c.IsClass("Priest") && c.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.SHADOW)
			.Select(c => new Card(c));
	}
}
