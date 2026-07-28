using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Battlecry: Add a random Outcast card to the left and right sides of your hand. They cost (2) less."
public class FelerinTheForgotten : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.FelerinTheForgotten;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.HasTag(GameTag.OUTCAST))
			.Select(c => new Card(c));
	}
}
