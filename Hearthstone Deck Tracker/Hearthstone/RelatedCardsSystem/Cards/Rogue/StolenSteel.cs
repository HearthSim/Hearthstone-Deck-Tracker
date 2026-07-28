using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Discover a weapon (from another class)."
public class StolenSteel : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.StolenSteel;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.WEAPON }
				&& !c.IsClass(playerClass) && !c.IsClass("Neutral"))
			.Select(c => new Card(c));
	}
}
