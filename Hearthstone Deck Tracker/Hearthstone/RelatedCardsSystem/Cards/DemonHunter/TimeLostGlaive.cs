using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Deathrattle: Get a random Demon from the past."
public class TimeLostGlaive : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.TimeLostGlaive;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsDemon())
			.Select(c => new Card(c));
	}
}
