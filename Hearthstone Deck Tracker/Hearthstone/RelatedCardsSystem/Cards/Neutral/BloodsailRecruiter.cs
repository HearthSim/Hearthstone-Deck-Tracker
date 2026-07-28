using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a Pirate."
public class BloodsailRecruiter : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.BloodsailRecruiter;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.HasRace(Race.PIRATE)
				&& (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
	}
}
