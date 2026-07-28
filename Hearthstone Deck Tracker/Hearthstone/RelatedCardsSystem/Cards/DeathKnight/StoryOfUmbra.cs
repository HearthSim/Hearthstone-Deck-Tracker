using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Discover a Deathrattle minion that costs (5) or more. Summon it and trigger its Deathrattle."
public class StoryOfUmbra : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.StoryOfUmbra;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: >= 5 }
				&& (c.IsClass(playerClass) || c.IsClass("Neutral")) && c.HasDeathrattle())
			.Select(c => new Card(c));
	}
}
