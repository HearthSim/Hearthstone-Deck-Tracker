using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Discover a 10-Cost minion from the past. Set its Cost to (1)."
public class FinalFrontier : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.FinalFrontier;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: 10 })
			.Select(c => new Card(c));
	}
}
