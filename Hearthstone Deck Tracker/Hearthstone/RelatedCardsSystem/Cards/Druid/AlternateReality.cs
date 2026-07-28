using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Replace your hand and deck with random Choose One cards from the past. They cost (1) less."
public class AlternateReality : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.AlternateReality;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.GetTag(GameTag.CHOOSE_ONE) > 0)
			.Select(c => new Card(c));
	}
}
