using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Get a random 5-Cost minion from the past."
public class FadingMemory : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.FadingMemory;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: 5 })
			.Select(c => new Card(c));
	}
}
