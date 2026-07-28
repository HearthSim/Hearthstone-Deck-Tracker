using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Discover a Murloc. If you play it this turn, also pick one of the others."
public class SubmergedMap : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.SubmergedMap;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && (c.IsClass(playerClass) || c.IsClass("Neutral")) && c.IsMurloc())
			.Select(c => new Card(c));
	}
}
