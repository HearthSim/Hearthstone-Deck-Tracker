using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "At the end of your turn, get a random minion with multiple minion types."
public class Tortotem : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Tortotem;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		// Multiple minion types: a secondary race, or race "All" (Amalgams).
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION }
				&& (c.SecondaryRace != Race.INVALID || c.Race == Race.ALL))
			.Select(c => new Card(c));
	}
}
