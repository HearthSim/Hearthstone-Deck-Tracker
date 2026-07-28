using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Discover a minion with a Dark Gift. It costs (2) less."
public class Cremate : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.Cremate;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c =>
				c is { Type: CardType.MINION } &&
				(c.IsClass(playerClass) || c.IsClass("Neutral"))
			)
			.Select(c => new Card(c));
	}
}
