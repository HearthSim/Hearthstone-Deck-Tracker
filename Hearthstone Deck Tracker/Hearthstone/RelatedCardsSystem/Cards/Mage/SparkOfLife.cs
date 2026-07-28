using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Choose One - Discover a Mage spell; or Discover a Druid spell."
public class SparkOfLife : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.SparkOfLife;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL } && (c.IsClass("Mage") || c.IsClass("Druid")))
			.Select(c => new Card(c));
	}
}
