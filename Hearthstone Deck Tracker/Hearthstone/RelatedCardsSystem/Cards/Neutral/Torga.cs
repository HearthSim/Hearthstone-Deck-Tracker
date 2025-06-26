using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class Torga: ICardWithHighlight
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Torga;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck)
	{
		var kindreds = deck
				.Where(c => c.HasTag(GameTag.KINDRED)).ToList();

		var isMinionKindredTarget = card.TypeEnum == CardType.MINION &&
	        kindreds.Where(c => c.TypeEnum == CardType.MINION)
	        .SelectMany(m => new List<Race?> { m.RaceEnum, m.SecondaryRaceEnum }.Where(
				race => race.HasValue && race.Value != Race.INVALID
		    )).Cast<Race>().Any(card.HasRace);

		var isSpellKindredTarget = card.TypeEnum == CardType.SPELL &&
		    kindreds.Where(c => c.TypeEnum == CardType.SPELL)
			.Select(spell => spell.GetTag(GameTag.SPELL_SCHOOL))
			.Where(spellSchool => spellSchool > 0)
			.Any(spellSchool => card.GetTag(GameTag.SPELL_SCHOOL) == spellSchool);

		return HighlightColorHelper.GetHighlightColor(
			card.HasTag(GameTag.KINDRED),
			isMinionKindredTarget || isSpellKindredTarget
		);
	}
}
