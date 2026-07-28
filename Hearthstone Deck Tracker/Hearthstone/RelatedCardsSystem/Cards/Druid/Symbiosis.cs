using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Discover a Choose One card from another class."
// "From another class" explicitly excludes the player's class and neutral cards.
public class Symbiosis : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.Symbiosis;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c =>
				!c.IsClass(playerClass) &&
				!c.IsClass("Neutral") &&
				c.HasTag(GameTag.CHOOSE_ONE)
			)
			.Select(c => new Card(c));
	}
}
