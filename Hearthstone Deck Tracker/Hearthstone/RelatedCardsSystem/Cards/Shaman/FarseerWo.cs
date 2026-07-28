using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Elusive After you cast a spell, Discover a Nature spell from the past."
public class FarseerWo : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.FarseerWo;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL }
				&& (c.IsClass(playerClass) || c.IsClass("Neutral"))
				&& c.GetTag(GameTag.SPELL_SCHOOL) == (int)SpellSchool.NATURE)
			.Select(c => new Card(c));
	}
}
