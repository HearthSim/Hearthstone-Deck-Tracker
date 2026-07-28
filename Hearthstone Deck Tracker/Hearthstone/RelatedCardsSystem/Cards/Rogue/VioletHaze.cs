using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Add 2 random Deathrattle cards to your hand."
public class VioletHaze : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.VioletHaze;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.HasDeathrattle())
			.Select(c => new Card(c));
	}
}
