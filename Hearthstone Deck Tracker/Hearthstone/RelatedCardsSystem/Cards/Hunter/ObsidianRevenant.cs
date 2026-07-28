using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Taunt. Deathrattle: Summon two random Deathrattle minions that cost (3) or less."
public class ObsidianRevenant : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.ObsidianRevenant;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: <= 3 } && c.HasDeathrattle())
			.Select(c => new Card(c));
	}
}
