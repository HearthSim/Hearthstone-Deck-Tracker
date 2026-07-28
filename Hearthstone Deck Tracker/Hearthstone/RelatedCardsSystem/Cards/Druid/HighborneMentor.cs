using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Battlecry: Get a 2/2 Pupil. Discover a spell that costs (7) or more from the past to teach it."
public class HighborneMentor : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.HighborneMentor;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL, Cost: >= 7 }
				&& (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
	}
}
