using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Discover a Demon that costs (5) or more with a Dark Gift."
public class Jumpscare : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.Jumpscare;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: >= 5 }
				&& c.IsDemon() && (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
	}
}
