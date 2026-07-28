using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Add a random 2-Cost and 3-Cost Beast to your hand."
public class PartnerAssignment : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.PartnerAssignment;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: 2 or 3 } && c.IsBeast())
			.Select(c => new Card(c));
	}
}
