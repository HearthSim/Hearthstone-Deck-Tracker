using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Discover a Draenei. If you played an adjacent card this turn, Discover another."
public class OrbitalSatellite : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.OrbitalSatellite;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && (c.IsClass(playerClass) || c.IsClass("Neutral")) && c.isDraenei())
			.Select(c => new Card(c));
	}
}
