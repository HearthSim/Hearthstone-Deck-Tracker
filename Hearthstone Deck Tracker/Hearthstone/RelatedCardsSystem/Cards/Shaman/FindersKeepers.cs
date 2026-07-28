using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Discover a card with Overload. Overload: (1)"
public class FindersKeepers : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.FindersKeepers;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => (c.IsClass(playerClass) || c.IsClass("Neutral")) && c.HasTag(GameTag.OVERLOAD))
			.Select(c => new Card(c));
	}
}
