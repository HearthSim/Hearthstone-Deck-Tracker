using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a Druid, Rogue, or Shaman card."
public class LotusAgentsGANGS : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.LotusAgentsGANGS;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.IsClass("Druid") || c.IsClass("Rogue") || c.IsClass("Shaman"))
			.Select(c => new Card(c));
	}
}

public class LotusAgentsWONDERS : LotusAgentsGANGS
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.LotusAgentsWONDERS;
}
