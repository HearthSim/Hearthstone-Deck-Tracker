using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Discover a Rush minion. Your next one costs (1) less."
public class AthleticStudies : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.AthleticStudies;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION }
				&& (c.IsClass(playerClass) || c.IsClass("Neutral")) && c.HasTag(GameTag.RUSH))
			.Select(c => new Card(c));
	}
}
