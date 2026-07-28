using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Battlecry: Get a random 1-Cost minion. Deathrattle: Get a random 1-Cost spell."
public class RaptorNestNurse : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.RaptorNestNurse;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		var minions = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: 1 })
			.Select(c => new Card(c));
		var spells = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL, Cost: 1 })
			.Select(c => new Card(c));
		return minions.Concat(spells);
	}
}
