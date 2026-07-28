using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Discover 2 Warlock cards."
public class TwistedKnowledge : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.TwistedKnowledge;
	public override int Picks() => 3;
	public override int EventCount() => 2;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.IsClass("Warlock"))
			.Select(c => new Card(c));
	}
}
