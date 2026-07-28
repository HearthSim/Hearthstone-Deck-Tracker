using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Battlecry: Get a random Overload card."
public class ShockHopper : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.ShockHopper;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.HasTag(GameTag.OVERLOAD))
			.Select(c => new Card(c));
	}
}
