using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Discover a Quickdraw card from another class."
public class StickUp : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.StickUp;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => !c.IsClass(playerClass) && !c.IsClass("Neutral") && c.HasTag(GameTag.QUICKDRAW))
			.Select(c => new Card(c));
	}
}
