using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Discover a Beast and an Undead. Swap their stats."
public class MismatchedFossils : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.MismatchedFossils;
	public override int Picks() => 3;
	public override int EventCount() => 2;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION }
				&& (c.IsClass(playerClass) || c.IsClass("Neutral")) && (c.IsBeast() || c.IsUndead()))
			.Select(c => new Card(c));
	}
}
