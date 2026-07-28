using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Summon two random 1-Cost minions from the past. Combo: With +1 Attack."
public class Flashback : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.Flashback;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: 1 })
			.Select(c => new Card(c));
	}
}
