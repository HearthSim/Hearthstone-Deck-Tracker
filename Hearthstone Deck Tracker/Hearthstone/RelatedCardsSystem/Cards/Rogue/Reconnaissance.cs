using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Discover a Deathrattle minion from another class. It costs (2) less."
public class Reconnaissance : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.Reconnaissance;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION }
				&& !c.IsClass(playerClass) && !c.IsClass("Neutral") && c.HasDeathrattle())
			.Select(c => new Card(c));
	}
}
