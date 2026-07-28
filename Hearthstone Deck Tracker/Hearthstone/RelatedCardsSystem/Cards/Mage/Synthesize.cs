using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Add a random 1, 2, and 3-Cost Elemental to your hand."
public class Synthesize : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.Synthesize;
	public override int Picks() => 1;
	public override int EventCount() => 3;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.Cost is 1 or 2 or 3 && c.IsElemental())
			.Select(c => new Card(c));
	}
}
