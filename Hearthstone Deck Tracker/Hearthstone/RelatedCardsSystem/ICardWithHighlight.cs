using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

public interface ICardWithHighlight : ICard
{
	HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck);
}

public interface ISpellSchoolTutor : ICardWithHighlight
{
	int[] TutoredSpellSchools { get; }
}
