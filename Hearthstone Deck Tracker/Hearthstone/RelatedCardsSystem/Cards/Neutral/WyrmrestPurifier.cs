using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Transform all Neutral cards in your deck into random cards from your class."
public class WyrmrestPurifier : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.WyrmrestPurifier;

	// Transforms an unpredictable number of deck cards; model as a single representative draw.
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.IsClass(playerClass) && !c.IsClass("Neutral"))
			.Select(c => new Card(c));
	}
}
