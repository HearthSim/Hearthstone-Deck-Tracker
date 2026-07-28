using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Discover an Outcast card. Your next one costs (1) less."
public class IllidariStudies : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.IllidariStudies;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c =>
				(c.IsClass(playerClass) || c.IsClass("Neutral")) &&
				c.HasTag(GameTag.OUTCAST)
			)
			.Select(c => new Card(c));
	}
}

public class IllidariStudiesCore : IllidariStudies
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.IllidariStudiesCore;
}
