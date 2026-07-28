using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Shuffle 5 random minions into your deck that cost (8) or more. Double their stats."
public class AzsharasTriumph : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.AzsharasTriumph;
	public override int Picks() => 1;
	public override int EventCount() => 5;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: >= 8 })
			.Select(c => new Card(c));
	}
}
